using GameGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DesignationCell : IDesignationCell
{
    private readonly MainGrid grid;
    public GroundPoint GroundPoint { get; }

    public Vector3 Position { get { return new Vector3(GroundPoint.Position.x, Height, GroundPoint.Position.y); } }

    public int Height { get; }

    private VoxelDesignationType designation;
    public VoxelDesignationType Designation
    {
        get => designation;
        set
        {
            designation = value;
            grid.SetCellFilled(this, value != VoxelDesignationType.Empty);
        }
    }

    public bool IsFilled { get { return designation != VoxelDesignationType.Empty; } }

    public DesignationCell CellBelow
    {
        get
        {
            if (Height == 0) return null;
            return GroundPoint.DesignationCells[Height - 1];
        }
    }

    public DesignationCell CellAbove
    {
        get
        {
            if (Height == grid.MaxHeight - 1) return null;
            return GroundPoint.DesignationCells[Height + 1];
        }
    }
    
    public IEnumerable<VisualCell> Visuals { get; private set; }

    public DesignationCell(MainGrid grid, GroundPoint groundPoint, int height)
    {
        this.grid = grid;
        GroundPoint = groundPoint;
        Height = height;
    }

    public void PopulateVisuals()
    {
        List<VisualCell> visuals = new List<VisualCell>();

        visuals.AddRange(GroundPoint.PolyConnections.Select(item => grid.GetVisualCell(item, Height)));

        if (Height < grid.MaxHeight - 1)
        {
            visuals.AddRange(GroundPoint.PolyConnections.Select(item => grid.GetVisualCell(item, Height + 1)));
        }
        Visuals = visuals;
    }
    
    public override string ToString()
    {
        return "(" + GroundPoint.Index + ", " + Height + ")";
    }
}