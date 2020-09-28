using UnityEngine;
using GridMaking;

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

        public int Index { get; }

        public MeshBuilderAnchorPoint(EasedPoint anchor, int index)
        {
            Anchor = anchor;
            Index = index;
        }
    }
}