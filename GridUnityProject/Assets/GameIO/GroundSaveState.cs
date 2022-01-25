﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GroundSaveState
    {
        public GroundPointBuilder[] Points;

        public GroundEdgeBuilder[] Edges;

        public GroundSaveState(MainGrid grid)
        {
            GroundPointBuilder[] points = grid.Points.Select(item => new GroundPointBuilder(item)).ToArray();
            GroundEdgeBuilder[] edges = grid.Edges.Select(item => new GroundEdgeBuilder(item)).ToArray();
            Points = points.ToArray();
            Edges = edges.ToArray();
        }

        internal static MainGrid LoadDefault()
        {
            GroundPointBuilder origin = new GroundPointBuilder(0, Vector2.zero);
            List<GroundPointBuilder> points = new List<GroundPointBuilder>() { origin };
            List<GroundEdgeBuilder> edges = new List<GroundEdgeBuilder>();
            for (int i = 0; i < 6; i++)
            {
                float theta = 60 * i * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                GroundPointBuilder newPoint = new GroundPointBuilder( i + 1, pos);
                points.Add(newPoint);
                if(i % 2 == 0)
                {
                    edges.Add(new GroundEdgeBuilder(newPoint.Index, origin.Index));
                }
            }
            for (int i = 0; i < 6; i++)
            {
                int startIndex = i + 1;
                int endIndex = ((i + 1) % 6) + 1;

                GroundEdgeBuilder edgeA = new GroundEdgeBuilder(points[startIndex].Index, points[endIndex].Index);
                edges.Add(edgeA);
            }
            return new MainGrid(points, edges);
        }
    }
}