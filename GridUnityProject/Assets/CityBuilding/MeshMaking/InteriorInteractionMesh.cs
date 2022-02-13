using Interiors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
    public class InteriorInteractionMesh : InteractionMesh
    {
        private readonly MeshCollider meshFilter;
        private readonly Interior interior;
        public InteriorInteractionMesh(Interior interior, GameObject gameObj)
        {
            meshFilter = gameObj.GetComponent<MeshCollider>();
            this.interior = interior;
        }
        public void UpdateMesh()
        {
            IEnumerable<IMeshContributor> meshContributors = GetMeshContributors().ToArray();
            UpdateMesh(meshContributors);
            meshFilter.sharedMesh = null;
            meshFilter.sharedMesh = Mesh; // Hack to force update
        }
        private IEnumerable<IMeshContributor> GetMeshContributors()
        {
            return interior.Cells.Select(item => new CellMeshContributor(item));
        }
    }
}
