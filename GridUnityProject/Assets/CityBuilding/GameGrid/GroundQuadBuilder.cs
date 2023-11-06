using System;
using UnityEngine;

namespace GameGrid
{
    [Serializable]
    public class GroundQuadBuilder
    {
        public int PointAIndex;
        public int PointBIndex;
        public int PointCIndex;
        public int PointDIndex;

        public GroundQuadBuilder(GroundQuad quad)
            : this(quad.Points[0].Index,
                  quad.Points[1].Index,
                  quad.Points[2].Index,
                  quad.Points[3].Index)
        { }

        public GroundQuadBuilder(
            int pointAIndex,
            int pointBIndex,
            int pointCIndex,
            int pointDIndex
            )
        {
            PointAIndex = pointAIndex;
            PointBIndex = pointBIndex;
            PointCIndex = pointCIndex;
            PointDIndex = pointDIndex;
        }
    }

}