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

    public class GridQuad
    {
        private readonly Dictionary<GridPoint, GridPoint> diagonalsTable;
        
        public ReadOnlyCollection<GridPoint> Points { get; }
        
        public ReadOnlyCollection<GridEdge> Edges { get; }

        public Vector2 Center { get { return (Points[0].Position + Points[1].Position + Points[2].Position + Points[3].Position) / 4; } }

        public GridQuad(params GridEdge[] edges)
        {
            if (edges.Length != 4)
            {
                throw new ArgumentException("Can't create a GridQuad with " + edges.Length + " edges");
            }
            Edges = edges.ToList().AsReadOnly();
            Points = GetPoints();
            diagonalsTable = GetDiagonalsTable();
        }

        private ReadOnlyCollection<GridPoint> GetPoints()
        {
            HashSet<GridPoint> rawPoints = new HashSet<GridPoint>
            {
                Edges[0].PointA,
                Edges[0].PointB,
                Edges[1].PointA,
                Edges[1].PointB,
                Edges[2].PointA,
                Edges[2].PointB,
                Edges[3].PointA,
                Edges[3].PointB,
            };
            if (rawPoints.Count != 4)
            {
                throw new ArgumentException("Can't create a GridQuad with " + rawPoints.Count + " unique points");
            }
            return GetSortedPoints(rawPoints.ToArray()).ToList().AsReadOnly();
        }

        public GridPoint GetDiagonalPoint(GridPoint point)
        {
            return diagonalsTable[point];
        }

        private IEnumerable<GridPoint> GetSortedPoints(GridPoint[] rawPoints)
        {
            Vector2 center = (rawPoints[0].Position + rawPoints[1].Position + rawPoints[2].Position + rawPoints[3].Position) / 4;
            return rawPoints.OrderByDescending(item => Vector2.SignedAngle(Vector2.up, item.Position - center));
        }

        private Dictionary<GridPoint, GridPoint> GetDiagonalsTable()
        {
            Dictionary<GridPoint, GridPoint> ret = new Dictionary<GridPoint, GridPoint>();
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