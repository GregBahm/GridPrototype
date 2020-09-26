using UnityEngine;

namespace GridMaking
{
    public class TrianglePoint
    {
        private static Vector2 xUnitOffset = new Vector2(1, -1.73f).normalized;

        public int GridX { get; }
        public int GridY { get; }
        public float PosX { get; }
        public float PosY { get; }
        public bool IsBorder { get; }

        public TrianglePoint(int gridX, int gridY, bool isBorder)
        {
            GridX = gridX;
            GridY = gridY;
            PosY = xUnitOffset.y * gridY;
            PosX = gridX + (xUnitOffset.x * gridY);
            IsBorder = isBorder;
        }

        public override string ToString()
        {
            return "(" + GridX + "," + GridY + ")";
        }
    }
}