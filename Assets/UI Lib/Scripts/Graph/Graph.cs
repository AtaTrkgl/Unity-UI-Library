﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

# if UNITY_EDITOR
using UnityEditor;
#endif

namespace UILib
{
    public class Graph : MonoBehaviour
    {
        #region Unity Editor
#if UNITY_EDITOR
        [MenuItem("GameObject/UI/UI Lib/Graph")]
        public static void AddGraph()
        {
            GameObject obj = Instantiate(Resources.Load<GameObject>("Graph"));
            obj.transform.SetParent(Selection.activeGameObject.transform, false);
            obj.name = "New Graph";
        }
#endif
        #endregion

        [SerializeField] private Sprite markerSprite;
        [SerializeField] private Sprite barSprite;
        [SerializeField] private float barPadding = 53f;
        [SerializeField] private float markerScale = 11f;
        [SerializeField] private float markerConnectorScale = 3f;
        [SerializeField] private bool addTooltips = true;
        public float maxHeight = 100;

        private RectTransform graphContainer;
        private RectTransform markersParent;
        private RectTransform connectorsParrent;

        private void Awake()
        {
            graphContainer = transform.Find("Graphics Container").GetComponent<RectTransform>();
            markersParent = graphContainer.transform.Find("Markers").GetComponent<RectTransform>();
            connectorsParrent = graphContainer.transform.Find("Connectors").GetComponent<RectTransform>();
        }

        public void ShowGraph(List<int> valueList, GraphType graphType, bool ignoreMaxHeight)
        {
            switch (graphType)
            {
                case GraphType.Line:
                    CreateSimpleGraph(valueList, ignoreMaxHeight, false);
                    break;
                case GraphType.Scatter:
                    CreateSimpleGraph(valueList, ignoreMaxHeight, true);
                    break;
                case GraphType.Bar:
                    CreateBarGraph(valueList, ignoreMaxHeight);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// This function can handle both line & scatter plots
        /// </summary>
        /// <param name="valueList"> values</param>
        /// <param name="ignoreMaxHeight"> if the variable max height should be ignored or not</param>
        /// <param name="isScaterGraph"></param>
        private void CreateSimpleGraph(List<int> valueList, bool ignoreMaxHeight, bool isScaterGraph)
        {
            float graphHeight = graphContainer.sizeDelta.y - 2 * markerScale;

            float yMax = maxHeight;

            int greatestIndex = 0;
            for (int i = 0; i < valueList.Count; i++)
            {
                if (valueList[i] > valueList[greatestIndex])
                    greatestIndex = i;
            }
            if (ignoreMaxHeight || valueList[greatestIndex] > maxHeight)
                yMax = valueList[greatestIndex];

            float xSize = graphContainer.sizeDelta.x / (valueList.Count > 1 ? valueList.Count : 2);
            GameObject lastMarker = null;

            for (int i = 0; i < valueList.Count; i++)
            {
                valueList[i] = (int) Mathf.Clamp(valueList[i],0f , yMax);
                float xPos = (i + .5f) * xSize;
                float yPos = (valueList[i] / yMax) * graphHeight + markerScale;
                var marker = CreateMarker(new Vector2(xPos, yPos), addTooltips, $"{i + 1}, {valueList[i]}");
                if (lastMarker != null && !isScaterGraph)
                {
                    CreateMarkerConnection(lastMarker.GetComponent<RectTransform>().anchoredPosition,
                        marker.GetComponent<RectTransform>().anchoredPosition);
                }
                lastMarker = marker;
            }
        }
        private void CreateBarGraph(List<int> valueList, bool ignoreMaxHeight)
        {
            float graphHeight = graphContainer.sizeDelta.y - 2 * markerScale;

            float yMax = maxHeight;

            int greatestIndex = 0;
            for (int i = 0; i < valueList.Count; i++)
            {
                if (valueList[i] > valueList[greatestIndex])
                    greatestIndex = i;
            }
            if (ignoreMaxHeight || valueList[greatestIndex] > maxHeight)
                yMax = valueList[greatestIndex];

            float xSize = (graphContainer.sizeDelta.x - 2 * barPadding) / (valueList.Count > 1 ? valueList.Count : 2);

            for (int i = 0; i < valueList.Count; i++)
            {
                valueList[i] = (int)Mathf.Clamp(valueList[i], 0f, yMax);
                float xPos = (i + .5f) * xSize + barPadding;
                float yPos = (valueList[i] / yMax) * graphHeight + markerScale;
                CreateBar(yPos, xPos, xSize/2);
            }
        }

        //Linear & Scatter Plots
        private void CreateMarkerConnection(Vector2 markerPosA, Vector2 markerPosB)
        {
            GameObject gameObj = new GameObject("markerConnection", typeof(Image));
            gameObj.transform.SetParent(connectorsParrent, false);
            RectTransform rectTrans = gameObj.GetComponent<RectTransform>();
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.zero;
            rectTrans.pivot = Vector2.zero;
            rectTrans.sizeDelta = new Vector2(Vector2.Distance(markerPosA, markerPosB), markerConnectorScale);
            rectTrans.anchoredPosition = markerPosA;

            float angle = Mathf.Atan2(markerPosB.y - markerPosA.y, markerPosB.x - markerPosA.x);
            rectTrans.localEulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
        }
        private GameObject CreateMarker(Vector2 anchoredPos, bool addToolTip = false, string toolTipText = "")
        {
            var gameObj = new GameObject("marker", typeof(Image));
            gameObj.transform.SetParent(markersParent, false);
            gameObj.GetComponent<Image>().sprite = markerSprite;

            var rectTrans = gameObj.GetComponent<RectTransform>();
            rectTrans.anchoredPosition = anchoredPos;
            rectTrans.sizeDelta = new Vector2(markerScale, markerScale);
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.zero;

            if (addToolTip)
            {
                gameObj.AddComponent<TooltipTarget>();
                gameObj.GetComponent<TooltipTarget>().toolTip = toolTipText;
            }

            return gameObj;
        }
        //Bar Plots
        private void CreateBar(float height, float xPos, float width)
        {
            var gameObj = new GameObject("marker", typeof(Image));
            gameObj.transform.SetParent(markersParent, false);
            gameObj.GetComponent<Image>().sprite = barSprite;

            var rectTrans = gameObj.GetComponent<RectTransform>();
            rectTrans.anchoredPosition = new Vector2(xPos, 0);
            rectTrans.sizeDelta = new Vector2(markerScale, markerScale);
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.zero;
            rectTrans.pivot = new Vector2(.5f,0f);
            rectTrans.sizeDelta = new Vector2(width, height);
        }

        public enum GraphType : byte
        { 
            Line = 0,
            Scatter = 1,
            Bar = 2,
        }
    }
}