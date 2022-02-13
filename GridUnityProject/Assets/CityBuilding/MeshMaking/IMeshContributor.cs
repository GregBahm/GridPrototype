using System.Collections.Generic;

namespace MeshMaking
{
    public interface IMeshContributor
    {
        IEnumerable<IMeshBuilderPoint> Points { get; }

        IEnumerable<MeshBuilderTriangle> Triangles { get; }
    }
}