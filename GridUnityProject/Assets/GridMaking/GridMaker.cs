using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridMaking
{

    public class GridMaker : MonoBehaviour
    {
        [SerializeField]
        private int gridSize;
        [SerializeField]
        [Range(0, 1)]
        private float GridOrHex;
        [SerializeField]
        [Range(0, 1)]
        private float easingWeight;
        [SerializeField]
        [Range(0, 1)]
        private float easingBorderWeight;

        private BaseGrid baseGrid;
        private TesselatedGrid tesselatedGrid;
        private EasedGrid easedGrid;

        private void Start()
        {
            baseGrid = new BaseGrid(gridSize);
            tesselatedGrid = new TesselatedGrid(baseGrid);
            Vector2 centerPoint = GetCenterPoint();
            easedGrid = new EasedGrid(tesselatedGrid, centerPoint);
        }

        private Vector2 GetCenterPoint()
        {
            BasePoint point = new BasePoint(gridSize / 2, gridSize / 2, false, false);
            return new Vector2(point.PosX, point.PosY);
        }

        private void Update()
        {
            //DisplayBaseConnections();

            easedGrid.DoEase(easingWeight, easingBorderWeight, gridSize);
            DisplayEasedConnections();
        }

        private void DisplayEasedConnections()
        {
            foreach (var item in easedGrid.EasedEdges)
            {
                Vector3 start = new Vector3(item.PointA.CurrentPos.x, 0, item.PointA.CurrentPos.y);
                Vector3 end = new Vector3(item.PointB.CurrentPos.x, 0, item.PointB.CurrentPos.y);
                Debug.DrawLine(start, end);
            }
        }

        private void DisplayBaseConnections()
        {
            foreach (BaseEdge item in baseGrid.CulledEdges)
            {
                float scale = 20f / (gridSize - 1);
                float pointAX = Mathf.Lerp(item.PointA.GridX, item.PointA.PosX, GridOrHex);
                float pointAY = Mathf.Lerp(item.PointA.GridY, item.PointA.PosY, GridOrHex);
                float pointBX = Mathf.Lerp(item.PointB.GridX, item.PointB.PosX, GridOrHex);
                float pointBY = Mathf.Lerp(item.PointB.GridY, item.PointB.PosY, GridOrHex);
                Vector3 start = new Vector3(pointAX, 0, pointAY) * scale;
                Vector3 end = new Vector3(pointBX, 0, pointBY) * scale;
                Color color = item.IsBorderEdge ? Color.blue : Color.white;
                Debug.DrawLine(start, end, color);
            }
        }
    }
}