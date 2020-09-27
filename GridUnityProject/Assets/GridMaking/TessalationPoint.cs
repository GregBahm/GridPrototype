using UnityEngine;

namespace GridMaking
{
    class TessalationPoint
    {
        public Vector2 Pos { get; }
        public bool IsBorder { get; }

        public TessalationPoint(Vector2 pos, bool isBorder)
        {
            Pos = pos;
            IsBorder = isBorder;
        }
    }
}