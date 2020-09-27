using System.Linq;

namespace GridMaking
{
    public class BaseEdge
    {
        public BasePoint PointA { get; }
        public BasePoint PointB { get; }

        public string Key { get; }

        public BaseEdge(BasePoint pointA, BasePoint pointB)
        {
            PointA = pointA;
            PointB = pointB;
            Key = GetKey(pointA, pointB);
        }

        public BaseTriangle[] Triangles { get; } = new BaseTriangle[2];

        public bool IsBorderEdge { get { return PointA.IsBorder && PointB.IsBorder; } }

        public bool Delete { get; set; }

        public static string GetKey(BasePoint a, BasePoint b)
        {
            return "X:" + a.GridX + "Y:" + a.GridY + "to X:" + b.GridX + "Y:" + b.GridY;
        }

        public override string ToString()
        {
            return PointA.ToString() + " -> " + PointB.ToString();
        }
    }
}