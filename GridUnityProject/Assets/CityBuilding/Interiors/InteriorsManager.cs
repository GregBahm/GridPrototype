using MeshMaking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interiors
{
    public class InteriorsManager
    {
        private readonly GameObject newMeshPrefab;
        private readonly Dictionary<Interior, InteriorInteractionMesh> interactionMeshs;

        public InteriorsManager(GameObject newMeshPrefab)
        {
            interactionMeshs = new Dictionary<Interior, InteriorInteractionMesh>();
            this.newMeshPrefab = newMeshPrefab;
        }

        public void UpdateInteractionMesh(Interior interior)
        {
            if(!interactionMeshs.ContainsKey(interior))
            {
                GameObject newObj = GameObject.Instantiate(newMeshPrefab);
                interactionMeshs.Add(interior, new InteriorInteractionMesh(interior, newObj));
            }
            interactionMeshs[interior].UpdateMesh();
        }
    }
}
