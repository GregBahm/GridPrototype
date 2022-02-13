using Interiors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
    public class InteriorInteractionMesh : InteractionMesh
    {
        public MeshCollider Collider { get; }

        private readonly Interior interior;
        public InteriorInteractionMesh(Interior interior, GameObject gameObj)
        {
            Collider = gameObj.GetComponent<MeshCollider>();
            this.interior = interior;
        }
        public void UpdateMesh()
        {
            IEnumerable<IMeshContributor> meshContributors = GetMeshContributors().ToArray();
            UpdateMesh(meshContributors);
            Collider.sharedMesh = null;
            Collider.sharedMesh = Mesh; // Hack to force update
        }
        private IEnumerable<IMeshContributor> GetMeshContributors()
        {
            return interior.Cells.Select(item => new InteriorCellMeshContributor(item));
        }
    }
}
