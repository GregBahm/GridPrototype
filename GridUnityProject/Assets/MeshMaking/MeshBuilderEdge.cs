using UnityEngine;

namespace MeshBuilding
{
    public class MeshBuilderEdge : IMeshBuilderVert
    {
        public MeshBuilderAnchorPoint PointA { get; }
        public MeshBuilderAnchorPoint PointB { get; }

        public Vector3 VertPos
        {
            get
            {
                return (PointA.VertPos + PointB.VertPos) / 2;
            }
        }
        public int CenterIndex { get; }
        public string Key { get; }

        public MeshBuilderEdge(MeshBuilderAnchorPoint pointA, MeshBuilderAnchorPoint pointB, int index)
        {
            PointA = pointA;
            PointB = pointB;
            CenterIndex = index;
            Key = GetEdgeKey(pointA, pointB);
        }

        public static string GetEdgeKey(MeshBuilderAnchorPoint pointA, MeshBuilderAnchorPoint pointB)
        {
            return Mathf.Min(pointA.Index, pointB.Index) + " to " + Mathf.Max(pointA.Index, pointB.Index);
        }
    }
}