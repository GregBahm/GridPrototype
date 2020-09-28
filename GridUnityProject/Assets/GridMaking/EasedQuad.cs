using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridMaking
{
    public class EasedQuad
    {
        public EasedPoint[] Points { get; }

        public EasedQuad(IEnumerable<EasedPoint> points)
        {
            List<EasedPoint> asList = points.ToList();
            Vector2 center = (asList[0].BasePos + asList[1].BasePos + asList[2].BasePos + asList[3].BasePos) / 4;
            IOrderedEnumerable<EasedPoint> ordered = asList.OrderByDescending(item => GetAngleToCenter(item, center));
            Points = ordered.ToArray();
        }

        private static float GetAngleToCenter(EasedPoint point, Vector2 center)
        {
            return Vector2.SignedAngle(Vector2.up, point.BasePos - center);
        }
    }
}