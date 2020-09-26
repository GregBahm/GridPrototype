using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace GridMaking
{
    public class BaseTriangle : IPolygon
    {
        public IEnumerable<TrianglePoint> Points { get; }
        public TriangleEdge[] Edges { get; }

        public BaseTriangle(TriangleEdge a, TriangleEdge b, TriangleEdge c)
        {
            Edges = new TriangleEdge[3] { a, b, c };
            Points = new HashSet<TrianglePoint> { a.PointA, a.PointB, b.PointA, b.PointB, c.PointA, c.PointB };
        }
    }
}