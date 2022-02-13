using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
    public class ExteriorsInteractionMesh : InteractionMesh
    {
        public MeshCollider Collider { get; }

        public ExteriorsInteractionMesh(GameObject interactionMeshObject)
        {
            Collider = interactionMeshObject.GetComponent<MeshCollider>();
        }

        public void UpdateMesh(MainGrid grid)
        {
            IEnumerable<IMeshContributor> meshContributors = GetMeshContributors(grid).ToArray();
            UpdateMesh(meshContributors);

            Collider.sharedMesh = null; // Hack to force update
            Collider.sharedMesh = Mesh;
        }
        private IEnumerable<IMeshContributor> GetMeshContributors(MainGrid grid)
        {
            IEnumerable<IMeshContributor> groundContributor = grid.Points.Where(item => !item.DesignationCells[0].IsFilled).Select(item => new HorizontalMeshContributor(item));
            IEnumerable<IMeshContributor> contributors = grid.FilledCells.Select(item => new ExteriorCellMeshContributor(item));
            return groundContributor.Concat(contributors);
        }
    }
}
