using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Assets.Scripts.Utility.Api
{
    public class YouTubeHelper
    {
        #region Data Classes
        public class VodInfo
        {
            public string Quality;
            public string Fallback;
            public string Url;
            public string Itag;
            public string Type;

            public bool IsVideo()
            {
                if (string.IsNullOrEmpty(Type)) return false;

                var typeString = Type.Split(';')[0];

                return typeString.IndexOf("video") >= 0;
            }
        }

        public class StreamInfo
        {
            public int Bandwidth;
            public string Codecs;
            public string Resolution;
            public string Subtitles;
            public string ClosedCaptions;
            public string Url;
        }
        #endregion

        #region Constants
        private const string YOUTUBE_URL = @"https://www.youtube.com/get_video_info?video_id=";
        private const string REQUIRED_PARAMS = @"&el=info&ps=default&eurl=&gl=US&hl=en";
        private const string LIVE_HEADER = "hlsvp";
        private const string URLENCODED = "url_encoded_fmt_stream_map";
        public const string LIVE_TYPE = "live";
        public const string VOD_TYPE = "vod";
        #endregion

        #region Delegate Declarations
        public delegate void YoutubeHelperResult(bool isError, string type);
        public YoutubeHelperResult Callback;
        #endregion

        #region Members and Properties
        private List<VodInfo> _listOfVods;
        private List<StreamInfo> _listOfStreams;
        public VodInfo[] Vods
        {
            get { return _listOfVods.Where(v => v.IsVideo()).ToArray(); }
        }
        public StreamInfo[] Streams
        {
            get { return _listOfStreams.ToArray(); }
        }
        private readonly MonoBehaviour _monoBehaviour;
        #endregion

        public YouTubeHelper(MonoBehaviour mono, YoutubeHelperResult callback)
        {
            _monoBehaviour = mono;
            Callback = callback;
        }

        /// <summary>
        /// Begin the chain of events that eventually gets links to
        /// playable internet videos
        /// </summary>
        /// <param name="videoId"></param>
        public void BeginGetVideoUrl(string videoId)
        {
            _monoBehaviour.StartCoroutine(GetVideoInfo(videoId));
        }

        public IEnumerator GetVideoInfo(string videoId)
        {
            UnityWebRequest request = UnityWebRequest.Get(YOUTUBE_URL + videoId + REQUIRED_PARAMS);

            yield return request.Send();

            if (request.isError) throw new UnityException("[ERROR] no video info");

            ParseVideoInfo(request.downloadHandler.text);
        }

        private void ParseVideoInfo(string videoInfo)
        {
            bool hasVods = false;
            var queryStringList = videoInfo.Split('&');
            foreach (var queryString in queryStringList)
            {
                var escaped = WWW.UnEscapeURL(queryString);
                var key = GetKey(escaped);
                if (key == LIVE_HEADER)
                {
                    // this video is still LIVE
                    var value = GetValue(escaped);

                    _monoBehaviour.StartCoroutine(GetPlaylist(value));

                    return;
                }

                if (key == URLENCODED)
                {
                    var value = GetValue(escaped);

                    if (value.Length > 0)
                    {
                        // its a Video On Demand
                        hasVods = true;

                        var data = value.Split(',');
                        _listOfVods = GetVideoList(data);
                        Callback.Invoke(false, "vod");

                        break;
                    }
                }
            }

            // Not LIVE and has no static videos ???
            if (!hasVods) throw new UnityException("This video_id has no videos associated with it");
        }
        
        private IEnumerator GetPlaylist(string url, bool preferHighQuality = true)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            yield return request.Send();
            
            if (request.isError || request.downloadedBytes == 0) throw new UnityException("[ERROR] no video info");

            // parse playlist
            _listOfStreams = GetStreamList(request.downloadHandler.text);

            // inform listeners
            Callback.Invoke(request.isError, "live");
        }

        #region Helper Functions
        private List<VodInfo> GetVideoList(ICollection<string> list)
        {
            List<VodInfo> vods = new List<VodInfo>(list.Count);
            foreach (var entry in list)
            {
                vods.Add(
                    QueryStringToVodInfo(entry)
                    );
            }
            
            return vods;
        }

        private static List<StreamInfo> GetStreamList(string m3u8)
        {
            var data = m3u8.Split('#');
            List<StreamInfo> streams = new List<StreamInfo>(data.Length);

            foreach (var row in data)
            {
                var streamInfo = M3U8ToStreamInfo(row);
                if (streamInfo != null)
                {
                    streams.Add(streamInfo);
                }
            }

            return streams;
        }
        
        private static string GetKey(string queryStr)
        {
            return queryStr.Substring(0, queryStr.IndexOf('='));
        }

        private static string GetValue(string queryStr)
        {
            return queryStr.Substring(queryStr.IndexOf('=') + 1); 
        }

        private VodInfo QueryStringToVodInfo(string data)
        {
            var queryStrings = data.Split('&');
            VodInfo result = new VodInfo();
            foreach (var queryString in queryStrings)
            {
                var key = GetKey(queryString);
                var value = WWW.UnEscapeURL(GetValue(queryString));
                switch (key)
                {
                    case "fallback_host":   result.Fallback = value; break;
                    case "quality":         result.Quality  = value; break;
                    case "url":             result.Url      = value; break;
                    case "itag":            result.Itag     = value; break;
                    case "type":            result.Type     = value; break;
                }
            }
            return result;
        }

        private static StreamInfo M3U8ToStreamInfo(string data)
        {
            const string STREAM_INFO_PREFIX = "EXT-X-STREAM-INF:";
            const string BANDWIDTH = "BANDWIDTH";
            const string CODECS = "CODECS";
            const string RESOLUTION = "RESOLUTION";
            const string CLOSED_CAPTIONS = "CLOSED-CAPTIONS";
            var streamInfo = new StreamInfo();
            
            if (data.Length < 18 || data.Substring(0, 17) != STREAM_INFO_PREFIX) return null;
            var splitData = data.Split('\n');
            var metaData = splitData[0].Split(',');
            foreach (var property in metaData)
            {
                var index = property.IndexOf('=');
                if (index < 1) continue;
                var key = property.Substring(0, index);
                var value = property.Substring(index + 1);
                switch (key)
                {
                    case BANDWIDTH:
                        streamInfo.Bandwidth = int.Parse(value);
                        break;
                    case CODECS:
                        streamInfo.Codecs = value;
                        break;
                    case RESOLUTION:
                        streamInfo.Resolution = value;
                        break;
                    case CLOSED_CAPTIONS:
                        streamInfo.ClosedCaptions = value;
                        break;
                }
            }
            streamInfo.Url = splitData[1];
            
            return streamInfo;
        }
        
        #endregion
    }
}
