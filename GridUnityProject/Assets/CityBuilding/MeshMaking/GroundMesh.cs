using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
    public class GroundMesh : InteractionMesh
    {
        public GroundMesh(MainGrid mainGrid) : base(mainGrid)
        {
        }

        public void UpdateMesh()
        {
            IEnumerable<IMeshContributor> meshContributors = GetMeshContributors().ToArray();
            UpdateMesh(meshContributors);
        }
        private IEnumerable<IMeshContributor> GetMeshContributors()
        {
            return mainGrid.Points.Select(item => new HorizontalMeshContributor(item));
        }
    }
}
