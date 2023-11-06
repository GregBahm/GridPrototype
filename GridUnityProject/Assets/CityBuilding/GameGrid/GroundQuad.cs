using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

namespace GameGrid
{

    public class GroundQuad
    {
        private readonly Dictionary<GroundPoint, GroundPoint> diagonalsTable;
        
        public ReadOnlyCollection<GroundPoint> Points { get; }
        
        public ReadOnlyCollection<GroundEdge> Edges { get; }

        public Vector2 Center { get; }

        public GroundQuad(GroundPoint[] sortedPoints, GroundEdge[] sortedEdges, Vector2 center)
        {
            Points = sortedPoints.ToList().AsReadOnly();
            Edges = sortedEdges.ToList().AsReadOnly();
            Center = center;
            diagonalsTable = GetDiagonalsTable();
        }

        public GroundEdge GetEdge(GroundPoint pointA, GroundPoint pointB)
        {
            return Edges.First(edge => (edge.PointA == pointA && edge.PointB == pointB) 
            || (edge.PointA == pointB && edge.PointB == pointA));
        }

        public GroundPoint GetDiagonalPoint(GroundPoint point)
        {
            return diagonalsTable[point];
        }

        private Dictionary<GroundPoint, GroundPoint> GetDiagonalsTable()
        {
            Dictionary<GroundPoint, GroundPoint> ret = new Dictionary<GroundPoint, GroundPoint>();
            for (int i = 0; i < 4; i++)
            {
                int opposingIndex = (i + 2) % 4;
                ret.Add(Points[i], Points[opposingIndex]);
            }
            return ret;
        }

        public override string ToString()
        {
            return Points[0].Index + "," + Points[1].Index + "," + Points[2].Index + "," + Points[3].Index;
        }
    }

}