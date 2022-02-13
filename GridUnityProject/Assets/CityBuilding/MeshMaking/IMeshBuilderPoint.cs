using UnityEngine;

namespace MeshMaking
{
    public interface IMeshBuilderPoint
    {
        string Key { get; }
        Vector3 Position { get; }
        Vector2 Uv { get; }
        bool IsCellPoint { get; }
    }
}