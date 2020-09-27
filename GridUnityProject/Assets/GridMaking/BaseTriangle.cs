using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace GridMaking
{
    public class BaseTriangle : IPolygon
    {
        public IEnumerable<BasePoint> Points { get; }
        public BaseEdge[] Edges { get; }

        public BaseTriangle(BaseEdge a, BaseEdge b, BaseEdge c)
        {
            Edges = new BaseEdge[3] { a, b, c };
            Points = new HashSet<BasePoint> { a.PointA, a.PointB, b.PointA, b.PointB, c.PointA, c.PointB };
        }
    }
}