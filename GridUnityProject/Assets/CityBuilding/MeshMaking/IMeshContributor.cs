using System.Collections.Generic;

namespace MeshMaking
{
    interface IMeshContributor
    {
        IEnumerable<IMeshBuilderPoint> Points { get; }

        IEnumerable<MeshBuilderTriangle> Triangles { get; }
    }
}