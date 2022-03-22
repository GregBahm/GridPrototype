using MeshMaking;
using System.Collections.Generic;
using VoxelVisuals;

namespace Interiors
{
    public class Interior
    {
        private readonly GridInteriors registry;
        public IEnumerable<DesignationCell> Cells { get { return registry.GetFor(this); } }

        public Interior(GridInteriors registry)
        {
            this.registry = registry;
        }
    }
}