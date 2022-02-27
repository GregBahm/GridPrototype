using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoxelVisuals
{
    public class NeighborComponents
    {
        public VisualCell Up { get; }
        public VisualCell Down { get; }
        public VisualCell Forward { get; }
        public VisualCell Back { get; }
        public VisualCell Left { get; }
        public VisualCell Right { get; }

        public NeighborComponents(
            VisualCell up,
            VisualCell down,
            VisualCell forward,
            VisualCell back,
            VisualCell left,
            VisualCell right)
        {
            Up = up;
            Down = down;
            Forward = forward;
            Back = back;
            Left = left;
            Right = right;
        }
    }
}