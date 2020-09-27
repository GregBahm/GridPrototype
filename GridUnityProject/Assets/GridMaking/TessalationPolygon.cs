using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridMaking
{
    class TessalationPolygon
    {
        public TessalationBaseEdge[] BaseEdges { get; }
        public TessalationPoint SubPoint { get; }
        public IEnumerable<TessalationEdge> SubEdges { get; }

        public TessalationPolygon(IEnumerable<TessalationBaseEdge> edges)
        {
            BaseEdges = edges.ToArray();
            SubPoint = GetSubPoint();
            SubEdges = CreateSubEdges().ToArray();
        }

        private IEnumerable<TessalationEdge> CreateSubEdges()
        {
            foreach (TessalationBaseEdge edge in BaseEdges)
            {
                yield return new TessalationEdge(edge.SubPoint, SubPoint);
            }
        }

        private TessalationPoint GetSubPoint()
        {
            Vector2 pointPos = Vector2.zero;
            foreach (TessalationBaseEdge edge in BaseEdges)
            {
                pointPos += edge.SubPoint.Pos;
            }
            pointPos /= BaseEdges.Length;
            return new TessalationPoint(pointPos, false);
        }
    }
}