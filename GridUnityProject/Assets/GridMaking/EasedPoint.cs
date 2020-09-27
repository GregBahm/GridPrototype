using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridMaking
{
    public class EasedPoint
    {
        public Vector2 BasePos { get; }
        public Vector2 CurrentPos { get; set; }

        public bool IsBorder { get; }

        public List<EasedPoint> ConnectedPoints { get; } = new List<EasedPoint>();

        public EasedPoint(Vector2 basePos, bool isBorder)
        {
            BasePos = basePos;
            CurrentPos = basePos;
            IsBorder = isBorder;
        }
    }
}