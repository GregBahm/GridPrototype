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

        public GridPointBuilder[] Points;

        internal static MainGrid LoadDefaultGrid()
        {
            GridPointBuilder origin = new GridPointBuilder(0, Vector2.zero);
            List<GridPointBuilder> points = new List<GridPointBuilder>() { origin };
            List<GridEdgeBuilder> edges = new List<GridEdgeBuilder>();

            for (int i = 0; i < 6; i++)
            {
                float theta = 60 * i * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                GridPointBuilder newPoint = new GridPointBuilder( i + 1, pos);
                points.Add(newPoint);
                edges.Add(new GridEdgeBuilder(newPoint.Id, origin.Id));
            }
            for (int i = 0; i < 6; i++)
            {
                float theta = (60 * i + 30) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta) * 1.5f, Mathf.Sin(theta) * 1.5f);
                GridPointBuilder newPoint = new GridPointBuilder(i + 7, pos);
                points.Add(newPoint);
            }
            for (int i = 0; i < 6; i++)
            {
                float theta = 60 * i * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta) * 2f, Mathf.Sin(theta) * 2);
                GridPointBuilder newPoint = new GridPointBuilder(i + 13, pos);
                points.Add(newPoint);
            }
            for (int i = 0; i < 6; i++)
            {
                int startIndex = i + 1;
                int mid = i + 7;
                int endIndex = ((i + 1) % 6) + 1;

                int outerStart = i + 13;
                int outerEnd = ((i + 1) % 6) + 13;

                GridEdgeBuilder edgeA = new GridEdgeBuilder(points[startIndex].Id, points[mid].Id);
                GridEdgeBuilder edgeB = new GridEdgeBuilder(points[mid].Id, points[endIndex].Id);
                GridEdgeBuilder edgeC = new GridEdgeBuilder(points[outerStart].Id, points[mid].Id);
                GridEdgeBuilder edgeD = new GridEdgeBuilder(points[mid].Id, points[outerEnd].Id);
                edges.Add(edgeA);
                edges.Add(edgeB);
                edges.Add(edgeC);
                edges.Add(edgeD);
            }
            return new MainGrid(points, edges);
        }

        public GridEdgeBuilder[] Edges;

        public GridLoader(IEnumerable<GridPointBuilder> points, IEnumerable<GridEdgeBuilder> edges)
        {
            Points = points.ToArray();
            Edges = edges.ToArray();
        }

        public static MainGrid LoadGrid()
        {
            return LoadDefaultGrid();
            string data = PlayerPrefs.GetString(SaveFilePath);
            if(string.IsNullOrWhiteSpace(data))
            {
                Debug.Log("No save data found. Loading default grid");
                return LoadDefaultGrid();
            }
            GridLoader gridLoader = JsonUtility.FromJson<GridLoader>(data);
            return new MainGrid(gridLoader.Points, gridLoader.Edges);
        }
        public static void SaveGrid(MainGrid grid)
        {
            GridPointBuilder[] points = grid.Points.Select(item => new GridPointBuilder(item)).ToArray();
            GridEdgeBuilder[] edges = grid.Edges.Select(item => new GridEdgeBuilder(item)).ToArray();
            GridLoader loader = new GridLoader(points, edges);
            string asJson = JsonUtility.ToJson(loader);
            PlayerPrefs.SetString(SaveFilePath, asJson);
            PlayerPrefs.Save();
        }
    }

}