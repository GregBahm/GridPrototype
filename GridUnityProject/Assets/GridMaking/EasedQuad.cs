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
            Points = points.ToArray();
        }
    }
}