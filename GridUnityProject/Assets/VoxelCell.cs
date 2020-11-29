using GameGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelCell
{
    private readonly MainGrid grid;
    public GroundPoint GroundPoint { get; }

    public Vector3 CellPosition { get { return new Vector3(GroundPoint.Position.x, Height, GroundPoint.Position.y); } }

    public int Height { get; }

    public bool Filled
    {
        get { return grid.IsFilled(this); }
        set
        {
            grid.SetCellFilled(this, value);
        }
    }

    public VoxelCell CellBelow
    {
        get
        {
            if (Height == 0) return null;
            return GroundPoint.Voxels[Height - 1];
        }
    }

    public VoxelCell CellAbove
    {
        get
        {
            if (Height == MainGrid.VoxelHeight - 1) return null;
            return GroundPoint.Voxels[Height + 1];
        }
    }

    public VoxelVisuals Visuals { get; }

    public VoxelCell(MainGrid grid, GroundPoint groundPoint, int height)
    {
        this.grid = grid;
        GroundPoint = groundPoint;
        Height = height;
        Visuals = new VoxelVisuals(this);
    }

    public override string ToString()
    {
        return "(" + GroundPoint.Index + ", " + Height + ")";
    }

    internal IEnumerable<VoxelCell> GetConnectedCells()
    {
        return GetConnectedCellsIncludingNulls().Where(item => item != null);
    }
    private IEnumerable<VoxelCell> GetConnectedCellsIncludingNulls()
    {
        yield return CellAbove;
        yield return CellBelow;
        foreach (GroundPoint point in GroundPoint.DirectConnections.Concat(GroundPoint.DiagonalConnections))
        {
            VoxelCell cell = point.Voxels[Height];
            yield return cell;
            yield return cell.CellBelow;
            yield return cell.CellAbove;
        }
    }
}
