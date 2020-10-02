using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GridLoader
    {
        private const string SaveFilePath = "TheSaveFile";

        public GridPointLoader[] Points;

        internal static MainGrid LoadDefaultGrid()
        {
            MainGrid ret = new MainGrid();
            GridPoint origin = new GridPoint(ret, 0, Vector2.zero);
            List<GridPoint> points = new List<GridPoint>() { origin };
            List<GridEdge> edges = new List<GridEdge>();

            for (int i = 0; i < 6; i++)
            {
                float theta = 60 * i * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                GridPoint newPoint = new GridPoint(ret, i + 1, pos);
                points.Add(newPoint);
                edges.Add(new GridEdge(ret, newPoint, origin));
            }
            for (int i = 0; i < 6; i++)
            {
                float theta = (60 * i + 30) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta) * 1.5f, Mathf.Sin(theta) * 1.5f);
                GridPoint newPoint = new GridPoint(ret, i + 7, pos);
                points.Add(newPoint);
            }
            for (int i = 0; i < 6; i++)
            {
                float theta = 60 * i * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta) * 2f, Mathf.Sin(theta) * 2);
                GridPoint newPoint = new GridPoint(ret, i + 13, pos);
                points.Add(newPoint);
            }
            for (int i = 0; i < 6; i++)
            {
                int startIndex = i + 1;
                int mid = i + 7;
                int endIndex = ((i + 1) % 6) + 1;

                int outerStart = i + 13;
                int outerEnd = ((i + 1) % 6) + 13;

                GridEdge edgeA = new GridEdge(ret, points[startIndex], points[mid]);
                GridEdge edgeB = new GridEdge(ret, points[mid], points[endIndex]);
                GridEdge edgeC = new GridEdge(ret, points[outerStart], points[mid]);
                GridEdge edgeD = new GridEdge(ret, points[mid], points[outerEnd]);
                edges.Add(edgeA);
                edges.Add(edgeB);
                edges.Add(edgeC);
                edges.Add(edgeD);
            }
            ret.AddToMesh(points, edges);
            return ret;
        }

        public GridEdgeLoader[] Edges;

        public GridLoader(IEnumerable<GridPointLoader> points, IEnumerable<GridEdgeLoader> edges)
        {
            Points = points.ToArray();
            Edges = edges.ToArray();
        }

        public static MainGrid LoadGrid()
        {
            MainGrid ret = new MainGrid();
            string data = PlayerPrefs.GetString(SaveFilePath);
            if(string.IsNullOrWhiteSpace(data))
            {
                Debug.Log("No save data found. Loading default grid");
                return LoadDefaultGrid();
            }
            GridLoader gridLoader = JsonUtility.FromJson<GridLoader>(data);
            Dictionary<int, GridPoint> lookupTable = gridLoader.Points.ToDictionary(item => item.Id, item => new GridPoint(ret, item.Id, item.Pos));
            IEnumerable<GridEdge> edges = gridLoader.Edges.Select(item => new GridEdge(ret, lookupTable[item.PointAId], lookupTable[item.PointBIds])).ToArray();
            ret.AddToMesh(lookupTable.Values, edges);
            return ret;
        }
        public static void SaveGrid(MainGrid grid)
        {
            GridPointLoader[] points = grid.Points.Select(item => new GridPointLoader(item)).ToArray();
            GridEdgeLoader[] edges = grid.Edges.Select(item => new GridEdgeLoader(item)).ToArray();
            GridLoader loader = new GridLoader(points, edges);
            string asJson = JsonUtility.ToJson(loader);
            PlayerPrefs.SetString(SaveFilePath, asJson);
            PlayerPrefs.Save();
        }
    }

}