using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Components
{
    public class CarouselPips : MonoBehaviour
    {
        public RectTransform CursorPrefab;
        public RectTransform PagePrefab;
        private Transform PipsParent;
        private LayoutGroup pipsLayoutGroup;
        private List<Transform> Pages;
        private RectTransform cursor;
        private float minPipPos;
        private float maxPipPos;
        private float xDist;

        // Use this for initialization
        void Awake ()
        {
            PipsParent = transform.GetChild(0);
        }

        public void Setup(int pages, int current)
        {
            Pages = new List<Transform>(pages);
            for (int i = pages; i > 0; i--)
            {
                var page = (RectTransform)Instantiate(PagePrefab, PipsParent);
                page.localScale = Vector3.one;
                
                Pages.Add(page.transform);
            }

            if (Pages.Count > 0)
            {
                var cursor = (RectTransform)Instantiate(CursorPrefab, transform);
                cursor.localScale = Vector3.one;
                this.cursor = cursor;
            }
        }

        /// <summary>
        /// Set value of pip cursor
        /// </summary>
        /// <param name="value">Scalar between 0 and 1</param>
        public void SetValue(float value)
        {
            minPipPos = Pages.First().position.x;
            maxPipPos = Pages.Last().position.x;
            xDist = maxPipPos - minPipPos;

            float clampedValue = Mathf.Clamp01(value);
            var pos = cursor.position;
            pos.y = Pages[0].position.y;
            pos.x = minPipPos + clampedValue*xDist;
            cursor.position = pos;
        }
    }
}
