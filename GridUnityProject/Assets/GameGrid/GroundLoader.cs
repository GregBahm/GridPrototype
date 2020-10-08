using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GroundLoader
    {
        private const string SaveFilePath = "TheSaveFile";

        public GroundPointBuilder[] Points;

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
                edges.Add(new GroundEdgeBuilder(newPoint.Index, origin.Index));
            }
            for (int i = 0; i < 6; i++)
            {
                float theta = (60 * i + 30) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta) * 1.5f, Mathf.Sin(theta) * 1.5f);
                GroundPointBuilder newPoint = new GroundPointBuilder(i + 7, pos);
                points.Add(newPoint);
            }
            for (int i = 0; i < 6; i++)
            {
                float theta = 60 * i * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(theta) * 2f, Mathf.Sin(theta) * 2);
                GroundPointBuilder newPoint = new GroundPointBuilder(i + 13, pos);
                points.Add(newPoint);
            }
            for (int i = 0; i < 6; i++)
            {
                int startIndex = i + 1;
                int mid = i + 7;
                int endIndex = ((i + 1) % 6) + 1;

                int outerStart = i + 13;
                int outerEnd = ((i + 1) % 6) + 13;

                GroundEdgeBuilder edgeA = new GroundEdgeBuilder(points[startIndex].Index, points[mid].Index);
                GroundEdgeBuilder edgeB = new GroundEdgeBuilder(points[mid].Index, points[endIndex].Index);
                GroundEdgeBuilder edgeC = new GroundEdgeBuilder(points[outerStart].Index, points[mid].Index);
                GroundEdgeBuilder edgeD = new GroundEdgeBuilder(points[mid].Index, points[outerEnd].Index);
                edges.Add(edgeA);
                edges.Add(edgeB);
                edges.Add(edgeC);
                edges.Add(edgeD);
            }
            return new MainGrid(points, edges);
        }

        public GroundEdgeBuilder[] Edges;

        public GroundLoader(IEnumerable<GroundPointBuilder> points, IEnumerable<GroundEdgeBuilder> edges)
        {
            Points = points.ToArray();
            Edges = edges.ToArray();
        }

        public static MainGrid Load()
        {
            string data = PlayerPrefs.GetString(SaveFilePath);
            if(string.IsNullOrWhiteSpace(data))
            {
                Debug.Log("No save data found. Loading default grid");
                return LoadDefault();
            }
            GroundLoader gridLoader = JsonUtility.FromJson<GroundLoader>(data);
            return new MainGrid(gridLoader.Points, gridLoader.Edges);
        }
        public static void Save(MainGrid grid)
        {
            GroundPointBuilder[] points = grid.Points.Select(item => new GroundPointBuilder(item)).ToArray();
            GroundEdgeBuilder[] edges = grid.Edges.Select(item => new GroundEdgeBuilder(item)).ToArray();
            GroundLoader loader = new GroundLoader(points, edges);
            string asJson = JsonUtility.ToJson(loader);
            PlayerPrefs.SetString(SaveFilePath, asJson);
            PlayerPrefs.Save();
        }
    }

}