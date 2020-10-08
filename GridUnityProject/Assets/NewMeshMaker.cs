using GameGrid;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HittestMesh
{
    private readonly Dictionary<int, IHitTarget> hitTables = new Dictionary<int, IHitTarget>();

    public Mesh Mesh { get; }

    public HittestMesh(Mesh mesh)
    {
        Mesh = mesh;
    }
    
    public IHitTarget GetHitTarget(int hitTringleIndex)
    {
        return hitTables[hitTringleIndex];
    }

    public void UpdateMesh(MainGrid grid)
    {
        IEnumerable<CellMeshContributor> cellMeshs = grid.FilledCells.Select(item => new CellMeshContributor(item)).ToArray();
        IEnumerable<GroundMeshContributor> groundMeshs = grid.Points.Where(item => !item.Voxels[0].Filled).Select(item => new GroundMeshContributor(item)).ToArray();

    }

    private class GroundMeshContributor
    {
        private readonly GroundPoint groundPoint;

        public GroundMeshContributor(GroundPoint emptyGroundPoint)
        {
            groundPoint = emptyGroundPoint;
        }
    }

    private class CellMeshContributor
    {
        private readonly VoxelCell cell;

        public CellMeshContributor(VoxelCell cell)
        {
            this.cell = cell;
        }
    }
}

public interface IHitTarget
{
    VoxelCell ActiveCell { get; }
    VoxelCell Source { get; }
}