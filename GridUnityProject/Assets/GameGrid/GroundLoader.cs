using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GroundLoader
    {
        private const string SaveFilePath = "TheSaveFile";

        public GroundPointBuilder[] Points;

        public GroundEdgeBuilder[] Edges;

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

        public static MainGrid Load(string json)
        {
            GroundLoader gridLoader = JsonUtility.FromJson<GroundLoader>(json);
            return new MainGrid(gridLoader.Points, gridLoader.Edges);
        }

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
            return Load(data);
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