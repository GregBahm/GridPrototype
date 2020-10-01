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

        private readonly List<MeshBuilderAnchorPoint> anchorConnections = new List<MeshBuilderAnchorPoint>();
        public IEnumerable<MeshBuilderAnchorPoint> DirectConnections { get { return anchorConnections; } }

        private HashSet<MeshBuilderAnchorPoint> tertiaryConnections = new HashSet<MeshBuilderAnchorPoint>();
        public IEnumerable<MeshBuilderAnchorPoint> TertiaryConnections { get { return tertiaryConnections; } }

        private List<MeshBuilderPoly> polyConnections = new List<MeshBuilderPoly>();
        public IEnumerable<MeshBuilderPoly> PolyConnections { get { return polyConnections; } }

        public MeshBuilderAnchorPoint(EasedPoint anchor, int index)
        {
            Anchor = anchor;
            Index = index;
        }

        public void AddEdgeConnection(MeshBuilderEdge edge)
        {
            if(edge.PointA != this)
            {
                anchorConnections.Add(edge.PointA);
            }
            if(edge.PointB != this)
            {
                anchorConnections.Add(edge.PointB);
            }
        }

        public void AddPolyConnection(MeshBuilderPoly poly)
        {
            polyConnections.Add(poly);
            foreach (MeshBuilderAnchorPoint item in poly.BasePoints.Where(item => item != this))
            {
                tertiaryConnections.Add(item);
            }
        }
    }
}
