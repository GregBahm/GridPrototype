using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GroundSaveState
    {
        public const int DefaultMaxHeight = 40;
        public int MaxHeight;

        public GroundPointBuilder[] Points;

        public GroundQuadBuilder[] Quads;

        public GroundSaveState(MainGrid grid)
        {
            MaxHeight = grid.MaxHeight;
            GroundPointBuilder[] points = grid.Points.Select(item => new GroundPointBuilder(item)).ToArray();
            GroundQuadBuilder[] edges = grid.Quads.Select(item => new GroundQuadBuilder(item)).ToArray();
            Points = points.ToArray();
            Quads = edges.ToArray();
        }
    }
}