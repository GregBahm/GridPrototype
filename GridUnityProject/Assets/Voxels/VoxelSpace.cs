using MeshBuilding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;

namespace Voxels
{

    public class VoxelSpace
    {
        public IEnumerable<VoxelColumn> Columns { get; }

        public VoxelSpace(IEnumerable<MeshBuilderAnchorPoint> basePoints, int maxHeight)
        {
            Dictionary<MeshBuilderAnchorPoint, VoxelColumn> lookupTable = basePoints.ToDictionary(
                item => item, item => new VoxelColumn(maxHeight));
            ConnectColumns(lookupTable);
            Columns = lookupTable.Values.ToArray();
            ConnectCells();
        }

        private void ConnectCells()
        {
            foreach (VoxelColumn column in Columns)
            {
                column.ConnectCells();
            }
        }

        private void ConnectColumns(Dictionary<MeshBuilderAnchorPoint, VoxelColumn> lookupTable)
        {
            foreach (var item in lookupTable)
            {
                IEnumerable<VoxelColumn> directConnections = item.Key.DirectConnections.Select(connection => lookupTable[connection]).ToArray();
                IEnumerable<VoxelColumn> indirectConnections = item.Key.TertiaryConnections.Select(connection => lookupTable[connection]).ToArray();
                item.Value.SetConnections(directConnections, indirectConnections);
            }
        }
    }
}
