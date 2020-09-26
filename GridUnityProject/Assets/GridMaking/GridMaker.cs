using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridMaking
{

    public class GridMaker : MonoBehaviour
    {
        [SerializeField]
        private int gridRows;
        [SerializeField]
        private int gridColumns;
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
            baseGrid = new BaseGrid(gridRows, gridColumns);
            tesselatedGrid = new TesselatedGrid(baseGrid);
            easedGrid = new EasedGrid(tesselatedGrid);
        }

        private void Update()
        {
            easedGrid.DoEase(easingWeight, easingBorderWeight);
            //DisplayBaseConnections();
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
            foreach (TriangleEdge item in baseGrid.CulledConnections)
            {
                Vector3 start = new Vector3(item.PointA.PosX, 0, item.PointA.PosY);
                Vector3 end = new Vector3(item.PointB.PosX, 0, item.PointB.PosY);
                Debug.DrawLine(start, end);
            }
        }
    }
}