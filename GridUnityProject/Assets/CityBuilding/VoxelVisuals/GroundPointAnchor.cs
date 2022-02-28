using UnityEngine;

namespace VoxelVisuals
{
    public struct GroundPointAnchor
    {
        public Vector2 AbsolutePosition { get; }
        public Vector2 XNormal { get; }
        public Vector2 YNormal { get; }

        public GroundPointAnchor(Vector2 absolutePosition, Vector2 xNormal, Vector2 yNormal)
        {
            AbsolutePosition = absolutePosition;
            XNormal = xNormal;
            YNormal = yNormal;
        }
    }
}