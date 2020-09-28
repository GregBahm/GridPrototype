using UnityEngine;
using GridMaking;
using System.Collections.Generic;
using System;

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

        public List<MeshBuilderPoly> Connections { get; } = new List<MeshBuilderPoly>();

        public MeshBuilderAnchorPoint(EasedPoint anchor, int index)
        {
            Anchor = anchor;
            Index = index;
        }
    }
}
