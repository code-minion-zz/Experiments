using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Components
{
    // Adapted from SwipeControl free asset on UAS - Kit
    public class CarouselController : MonoBehaviour
    {
        [Serializable]
        public class PageChangedEvent : UnityEvent<int> { }

        [Tooltip("Optional Pips object for displaying scrolling progress")]
        public CarouselPips Pips;
        [Tooltip("The pages we'll be flipping through")]
        public Transform[] Pages;
        [Tooltip("Affects distance between pages")]
        public float SwipeSmoothFactor = 0.1666667f;
        public float MinXPos = 0; // of camera
        public bool FlipPagesWithEdges = true;
        public bool AllowOverdrag = false;
        private float maxXPos;
        private SwipeControl swipeCtrl;
        private float xDist; // distance between min and max
        private float xDistRatio; // 1/pages
        private float xSizePage;

        private bool pageChanged = false;
        public PageChangedEvent OnPageChanged;

        // Use this for initialization
        void Start ()
        {
            maxXPos = Screen.width*Pages.Length;
            xDist = maxXPos - MinXPos; //calculate distance between min and max
            xDistRatio = 1f/(Pages.Length - 1f);
            xSizePage = 0.5f*xDist/Pages.Length;

            if (!swipeCtrl) swipeCtrl = gameObject.AddComponent<SwipeControl>();

            swipeCtrl.skipAutoSetup = true; //skip auto-setup, we'll call Setup() manually once we're done changing stuff
            swipeCtrl.clickEdgeToSwitch = FlipPagesWithEdges; //only swiping will be possible
            swipeCtrl.SetMouseRect(new Rect(0, 0, Screen.width, Screen.height)); //entire screen
            swipeCtrl.maxValue = Pages.Length - 1; //max value
            swipeCtrl.currentValue = 0;//swipeCtrl.maxValue; //current value set to max, so it starts from the end
            swipeCtrl.startValue = 0;//Mathf.RoundToInt(swipeCtrl.maxValue * 0.5f); //when Setup() is called it will animate from the end to the middle
            swipeCtrl.partWidth = (float)Screen.width / swipeCtrl.maxValue; //how many pixels do you have to swipe to change the value by one? in this case we make it dependent on the screen-width and the maxValue, so swiping from one edge of the screen to the other will scroll through all values.
            swipeCtrl.Setup();

            if (Pips != null)
            {
                Pips.Setup(Pages.Length, 0);
            }
        }
	
        // Update is called once per frame
        void Update () {

            float value = 0f;
            if (AllowOverdrag)
            {
                value = swipeCtrl.smoothValue;
            }
            else
            {
                value = Mathf.Clamp(swipeCtrl.smoothValue, 0, swipeCtrl.maxValue);
            }

            for (int i = 0; i < Pages.Length; i++)
            {
                var pos = Pages[i].position;
                
                pos.x = xSizePage + MinXPos + i * (xDist * SwipeSmoothFactor) - value * SwipeSmoothFactor * xDist;
                
                var fractional = value - (int)value;
                
                if (fractional < 0.05f || fractional > 0.95f)
                {
                    if (!pageChanged && OnPageChanged != null)
                    {
                        var page = Mathf.RoundToInt(value);
                        OnPageChanged.Invoke(page);
                        Debug.Log(page);
                    }
                    pageChanged = true;
                }
                else
                {
                    pageChanged = false;
                }

                Pages[i].position = pos;
                
            }

            if (Pips != null)
            {
                Pips.SetValue(value * xDistRatio);
            }
        }

        void SetAlpha(Image image, float value)
        {
            if (image == null) return;
            var color = image.color;
            color.a = value;
            image.color = color;
        }

        public void Skip()
        {
            swipeCtrl.currentValue = Pages.Length - 1;
        }
    }
}
