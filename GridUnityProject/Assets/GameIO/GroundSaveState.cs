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
        public GroundPointBuilder[] Points;

        public GroundQuadBuilder[] Quads;

        public GroundSaveState(GroundPointBuilder[] points, GroundQuadBuilder[] quads)
        {
            Points = points.ToArray();
            Quads = quads.ToArray();
        }

        public GroundSaveState(MainGrid grid)
        {
            Points = grid.Points.Select(item => new GroundPointBuilder(item)).ToArray();
            Quads = grid.Quads.Select(item => new GroundQuadBuilder(item)).ToArray();
        }
    }
}