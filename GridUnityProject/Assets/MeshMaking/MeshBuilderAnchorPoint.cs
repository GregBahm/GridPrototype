using UnityEngine;
using GridMaking;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MeshBuilding
{
    public class MeshBuilderAnchorPoint : IMeshBuilderVert
    {
        public EasedPoint Anchor { get; }
        public Vector3 VertPos
        {
            get
            {
                return new Vector3(Anchor.CurrentPos.x, 0, Anchor.CurrentPos.y);
            }
        }
        public Vector2 Uvs { get { return Vector2.zero; } }

        public int Index { get; }

        public List<MeshBuilderEdge> Connections { get; } = new List<MeshBuilderEdge>();

        public MeshBuilderAnchorPoint(EasedPoint anchor, int index)
        {
            Anchor = anchor;
            Index = index;
        }

        internal void SetSelectionMesh(Mesh selectionMesh)
        {
            selectionMesh.Clear();
            Vector3[] orderedVerts = GetSurroundingAnchorVerts();
            Vector3[] raisedVerts = orderedVerts.Select(item => item += Vector3.up).ToArray();
            selectionMesh.vertices = orderedVerts.Concat(raisedVerts).ToArray();
            selectionMesh.triangles = GetTriangles(orderedVerts.Length);
        }

        private int[] GetTriangles(int sides)
        {
            List<int> ret = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                ret.Add((i + 1) % sides);
                ret.Add(i + sides);
                ret.Add(i);

                ret.Add((i + 1) % sides + sides);
                ret.Add(i + sides);
                ret.Add((i + 1) % sides);
            }
            return ret.ToArray();
        }

        private Vector3[] GetSurroundingAnchorVerts()
        {
            List<Vector3> baseSurround = GetBaseSurroundingAnchorVerts();
            return baseSurround.OrderByDescending(item => Vector3.SignedAngle(item, Vector3.forward, Vector3.up)).ToArray();
        }

        private List<Vector3> GetBaseSurroundingAnchorVerts()
        {
            List<Vector3> ret = new List<Vector3>();
            foreach (MeshBuilderEdge edge in Connections)
            {
                if (edge.PointA != this)
                {
                    Vector3 selectionPos = (edge.PointA.VertPos - VertPos) / 2;
                    ret.Add(selectionPos);
                }
                if (edge.PointB != this)
                {
                    Vector3 selectionPos = (edge.PointB.VertPos - VertPos) / 2;
                    ret.Add(selectionPos);
                }
            }
            return ret;
        }
    }
}