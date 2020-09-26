using System.Collections.Generic;

namespace GridMaking
{
    interface IPolygon
    {
        TriangleEdge[] Edges { get; }
    }
}