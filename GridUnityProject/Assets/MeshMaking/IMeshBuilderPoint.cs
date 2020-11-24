using UnityEngine;

namespace MeshMaking
{
    interface IMeshBuilderPoint
    {
        string Key { get; }
        Vector3 Position { get; }
        Vector2 Uv { get; }
        bool IsCellPoint { get; }
    }
}