using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
    public class ExteriorsInteractionMesh : InteractionMesh
    {
        private readonly MeshCollider collider;

        public ExteriorsInteractionMesh(GameObject interactionMeshObject)
        {
            collider = interactionMeshObject.GetComponent<MeshCollider>();
        }

        public void UpdateMesh(MainGrid grid)
        {
            IEnumerable<IMeshContributor> meshContributors = GetMeshContributors(grid).ToArray();
            UpdateMesh(meshContributors);

            collider.sharedMesh = null; // Hack to force update
            collider.sharedMesh = Mesh;
        }
        private IEnumerable<IMeshContributor> GetMeshContributors(MainGrid grid)
        {
            IEnumerable<IMeshContributor> groundContributor = grid.Points.Where(item => !item.DesignationCells[0].IsFilled).Select(item => new HorizontalMeshContributor(item));
            IEnumerable<IMeshContributor> contributors = grid.FilledCells.Select(item => new CellMeshContributor(item));
            return groundContributor.Concat(contributors);
        }
    }
}
