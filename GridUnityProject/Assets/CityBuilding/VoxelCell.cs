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

    private SlotType designation;
    public SlotType Designation
    {
        get => designation;
        set
        {
            designation = value;
            grid.SetCellFilled(this, value != SlotType.Empty);
        }
    }

    public bool IsFilled { get { return designation != SlotType.Empty; } }

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
    
    public VoxelVisuals Visuals { get; private set; }

    public VoxelCell(MainGrid grid, GroundPoint groundPoint, int height)
    {
        this.grid = grid;
        GroundPoint = groundPoint;
        Height = height;
    }

    public void InitializeVisuals()
    {
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
