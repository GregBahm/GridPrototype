using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
    public class GroundMesh : InteractionMesh
    {
        public void UpdateMesh(MainGrid grid)
        {
            IEnumerable<IMeshContributor> meshContributors = GetMeshContributors(grid).ToArray();
            UpdateMesh(meshContributors);
        }
        private IEnumerable<IMeshContributor> GetMeshContributors(MainGrid grid)
        {
            return grid.Points.Select(item => new HorizontalMeshContributor(item));
        }
    }
}
