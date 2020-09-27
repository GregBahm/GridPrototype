using UnityEngine;

namespace GridMaking
{
    public class BasePoint
    {
        public static Vector2 HexUnitOffset = new Vector2(1, 1.73f).normalized;

        public int GridX { get; }
        public int GridY { get; }
        public float PosX { get; }
        public float PosY { get; }
        public bool IsBorder { get; }
        public bool IsWithinHex { get; }

        public BasePoint(int gridX, int gridY, bool isBorder, bool isWithinHex)
        {
            GridX = gridX;
            GridY = gridY;
            PosY = HexUnitOffset.y * gridY;
            PosX = gridX + (HexUnitOffset.x * gridY);
            IsBorder = isBorder;
            IsWithinHex = isWithinHex;
        }

        public override string ToString()
        {
            return "(" + GridX + "," + GridY + ")";
        }
    }
}