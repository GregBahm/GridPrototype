using System;
using System.Collections.Generic;
using System.Linq;

namespace GridMaking
{
    class BaseQuad : IPolygon
    {
        public TriangleEdge[] Edges { get; }

        public BaseQuad(IEnumerable<TriangleEdge> edges)
        {
            if(edges.Count() != 4)
            {
                throw new InvalidOperationException("Yo this quad got " + edges.Count() + " edges!");
            }
            Edges = edges.ToArray();
        }
    }
}