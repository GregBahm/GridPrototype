using System.Collections.Generic;
using UnityEngine;

namespace MeshBuilding
{
    public class MeshBuilderPoly : IMeshBuilderVert
    {
        public MeshBuilderAnchorPoint BasePointA { get; }
        public MeshBuilderAnchorPoint BasePointB { get; }
        public MeshBuilderAnchorPoint BasePointC { get; }
        public MeshBuilderAnchorPoint BasePointD { get; }

        public MeshBuilderEdge EdgeAB { get; }
        public MeshBuilderEdge EdgeBC { get; }
        public MeshBuilderEdge EdgeCD { get; }
        public MeshBuilderEdge EdgeDA { get; }
        public int CenterIndex { get; }

        public Vector3 VertPos
        {
            get
            {
                return (BasePointA.VertPos + BasePointB.VertPos + BasePointC.VertPos + BasePointD.VertPos) / 4;
            }
        }

        public IEnumerable<int> Triangles { get; }

        public MeshBuilderPoly(MeshBuilderAnchorPoint basePointA,
            MeshBuilderAnchorPoint basePointB,
            MeshBuilderAnchorPoint basePointC,
            MeshBuilderAnchorPoint basePointD,
            MeshBuilderEdge edgeAB,
            MeshBuilderEdge edgeBC,
            MeshBuilderEdge edgeCD,
            MeshBuilderEdge edgeDA,
            int centerIndex)
        {
            BasePointA = basePointA;
            BasePointB = basePointB;
            BasePointC = basePointC;
            BasePointD = basePointD;
            EdgeAB = edgeAB;
            EdgeBC = edgeBC;
            EdgeCD = edgeCD;
            EdgeDA = edgeDA;
            CenterIndex = centerIndex;
            Triangles = GetTriangle();
        }

        private IEnumerable<int> GetTriangle()
        {
            List<int> ret = new List<int>();
            ret.Add(CenterIndex);
            ret.Add(BasePointA.Index);
            ret.Add(EdgeAB.CenterIndex);

            ret.Add(CenterIndex);
            ret.Add(EdgeAB.CenterIndex);
            ret.Add(BasePointB.Index);

            ret.Add(CenterIndex);
            ret.Add(BasePointB.Index);
            ret.Add(EdgeBC.CenterIndex);

            ret.Add(CenterIndex);
            ret.Add(EdgeBC.CenterIndex);
            ret.Add(BasePointC.Index);

            ret.Add(CenterIndex);
            ret.Add(BasePointC.Index);
            ret.Add(EdgeCD.CenterIndex);

            ret.Add(CenterIndex);
            ret.Add(EdgeCD.CenterIndex);
            ret.Add(BasePointD.Index);

            ret.Add(CenterIndex);
            ret.Add(BasePointD.Index);
            ret.Add(EdgeDA.CenterIndex);

            ret.Add(CenterIndex);
            ret.Add(EdgeDA.CenterIndex);
            ret.Add(BasePointA.Index);

            return ret;
        }
    }
}