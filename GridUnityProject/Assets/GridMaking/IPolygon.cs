using System.Collections.Generic;

namespace GridMaking
{
    interface IPolygon
    {
        BaseEdge[] Edges { get; }
    }
}