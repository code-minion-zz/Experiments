using System.Linq;
using Assets.Scripts.Utility.Api;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Utility
{
    public class YoutubeLoader : MonoBehaviour
    {
        public string VideoId;
        private YouTubeHelper helper;
        public MediaPlayerCtrl mediaPlayer;
        public static bool Ready;

        // Use this for initialization
        void Start () {
            helper = new YouTubeHelper(this, OnRetrievedStreamUrl);
            BeginGetVideoUrl();
        }

        void BeginGetVideoUrl()
        {
            helper.BeginGetVideoUrl(VideoId);
        }

        void OnRetrievedStreamUrl(bool isError, string type)
        {
            string url = null;

            switch (type)
            {
                case YouTubeHelper.LIVE_TYPE:
                    url = helper.Streams.Last().Url;
                    break;
                case YouTubeHelper.VOD_TYPE:
                    url = helper.Vods.Last().Url;
                    break;
            }

            if (!string.IsNullOrEmpty(url))
            {
                Ready = true;
                mediaPlayer.Load(url);
            }
        }
    }
}
