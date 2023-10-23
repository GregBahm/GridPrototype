using GameGrid;
using MeshMaking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interiors
{
    public class InteriorsManager
    {
        private readonly MainGrid mainGrid;
        private readonly GameObject newMeshPrefab;
        private readonly Dictionary<Interior, InteriorInteractionMesh> interactionMeshs;

        public InteriorsManager(MainGrid mainGrid, GameObject newMeshPrefab)
        {
            this.mainGrid = mainGrid;
            this.newMeshPrefab = newMeshPrefab;
            interactionMeshs = new Dictionary<Interior, InteriorInteractionMesh>();
        }

        public InteriorInteractionMesh GetMeshFor(Interior interior)
        {
            return interactionMeshs[interior];
        }

        public void UpdateInteractionMesh(Interior interior)
        {
            if(!interactionMeshs.ContainsKey(interior))
            {
                GameObject newObj = Object.Instantiate(newMeshPrefab);
                interactionMeshs.Add(interior, new InteriorInteractionMesh(mainGrid, interior, newObj));
            }
            interactionMeshs[interior].UpdateMesh();
        }
    }
}
