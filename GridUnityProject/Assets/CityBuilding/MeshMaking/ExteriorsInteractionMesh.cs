using GameGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelVisuals;

namespace MeshMaking
{
    public class ExteriorsInteractionMesh : InteractionMesh
    {
        public MeshCollider Collider { get; }

        private Dictionary<DesignationCell, IMeshContributor> cellContributors;

        public ExteriorsInteractionMesh(MainGrid mainGrid, GameObject interactionMeshObject)
            :base(mainGrid)
        {
            cellContributors = new Dictionary<DesignationCell, IMeshContributor>();
            Collider = interactionMeshObject.GetComponent<MeshCollider>();
        }

        public void UpdateMesh(DesignationCell modifiedCell)
        {
            UpdateCellContributors(modifiedCell);
            UpdateMesh(cellContributors.Values);

            Collider.sharedMesh = null; // Hack to force update
            Collider.sharedMesh = Mesh;
        }

        private void UpdateCellContributors(DesignationCell modifiedCell)
        {
            bool isFilled = modifiedCell.IsFilled;
            if (!isFilled && cellContributors.ContainsKey(modifiedCell))
            {
                // Need to remove cell from contributors
                cellContributors.Remove(modifiedCell);
            }
            if (isFilled && !cellContributors.ContainsKey(modifiedCell))
            {
                // Need to add cell to contribotors
                cellContributors.Add(modifiedCell, new ExteriorCellMeshContributor(modifiedCell));
            }
        }

        public void RebuildMesh()
        {
            IEnumerable<IMeshContributor> meshContributors = GetMeshContributors().ToArray();
            UpdateMesh(meshContributors);

            Collider.sharedMesh = null; // Hack to force update
            Collider.sharedMesh = Mesh;
        }
        private IEnumerable<IMeshContributor> GetMeshContributors()
        {
            cellContributors = GetExteriorCellContributors();
            return cellContributors.Values;
        }

        private Dictionary<DesignationCell, IMeshContributor> GetExteriorCellContributors()
        {
            Dictionary<DesignationCell, IMeshContributor> ret = new Dictionary<DesignationCell, IMeshContributor>();
            foreach (DesignationCell filledCell in mainGrid.FilledCells)
            {
                ret.Add(filledCell, new ExteriorCellMeshContributor(filledCell));
            }
            return ret;
        }
    }
}
