using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelVisuals
{
    public class GroundPointAnchors
    {
        public GroundPoint BasePoint { get; }
        private readonly Dictionary<GroundPoint, Vector2> normalDictionary;

        public GroundPointAnchors(GroundPoint basePoint)
        {
            BasePoint = basePoint;
            normalDictionary = BasePoint.DirectConnections.ToDictionary(item => item, item => GetNormal(item));
        }

        private Vector2 GetNormal(GroundPoint connectedPoint)
        {
            Vector2 normA = (BasePoint.Position - connectedPoint.Position).normalized;
            GroundPoint mostOppositePoint = GetMostOpposite(connectedPoint, normA, BasePoint.DirectConnections);
            if (mostOppositePoint != null)
            {
                Vector2 normB = (BasePoint.Position - mostOppositePoint.Position).normalized;
                normB *= -1;
                return (normA + normB).normalized;
            }
            else
            {
                return normA;
            }
        }

        private GroundPoint GetMostOpposite(GroundPoint pointA, Vector2 pointANorm, IEnumerable<GroundPoint> directConnections)
        {
            GroundPoint ret = null;
            float maxAngle = 0;
            foreach (GroundPoint pointB in directConnections)
            {
                Vector2 pointBNorm = (BasePoint.Position - pointB.Position).normalized;
                float angle = Vector2.Angle(pointANorm, pointBNorm);
                if (angle > 90f && angle > maxAngle)
                {
                    maxAngle = angle;
                    ret = pointB;
                }
            }
            return ret;
        }

        public GroundPointAnchor GetAnchorFor(GroundQuad connectedQuad)
        {
            GroundPoint diagonalPoint = connectedQuad.GetDiagonalPoint(BasePoint);
            GroundPoint[] adjacentPoints = connectedQuad.Points.Where(point => point != BasePoint && point != diagonalPoint).ToArray();

            Vector2 normA = normalDictionary[adjacentPoints[0]];
            Vector2 normB = normalDictionary[adjacentPoints[1]];
            // Need to sort these in some way...

            return new GroundPointAnchor(BasePoint.Position, normA, normB);
        }
    }
}