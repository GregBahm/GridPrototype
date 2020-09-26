using System.Linq;

namespace GridMaking
{
    public class TriangleEdge
    {
        public TrianglePoint PointA { get; }
        public TrianglePoint PointB { get; }

        public string Key { get; }

        public TriangleEdge(TrianglePoint pointA, TrianglePoint pointB)
        {
            PointA = pointA;
            PointB = pointB;
            Key = GetKey(pointA, pointB);
        }

        public BaseTriangle[] Triangles { get; } = new BaseTriangle[2];

        public bool IsBorderEdge { get { return Triangles.All(item => item != null); } }

        public bool Delete { get; set; }

        public static string GetKey(TrianglePoint a, TrianglePoint b)
        {
            return "X:" + a.GridX + "Y:" + a.GridY + "to X:" + b.GridX + "Y:" + b.GridY;
        }

        public override string ToString()
        {
            return PointA.ToString() + " -> " + PointB.ToString();
        }
    }
}