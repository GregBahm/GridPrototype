using UnityEngine;

namespace MeshBuilding
{
    public interface IMeshBuilderVert
    {
        Vector3 VertPos { get; }
        Vector2 Uvs { get; }
    }
}