using System;
using System.Collections.Generic;
using System.Linq;
using GameGrid;
using UnityEngine;

public class VisualCell
{
    private readonly MainGrid grid;

    public VisualCellOption Contents { get; set; }

    public GroundQuad Quad { get; }

    private readonly DesignationCell[,,] designationCells;

    public Vector3 ContentPosition { get; }

    public NeighborComponents Neighbors { get; private set; }

    public int Height { get; }

    private readonly Vector3[] anchors;

    public VisualCell(MainGrid grid, GroundQuad quad, int height)
    {
        this.grid = grid;
        Quad = quad;
        Height = height;
        designationCells = GetDesignationCells();
        ContentPosition = GetContentPosition();
        anchors = new Vector3[]
        {
            designationCells[0, 0, 0].Position,
            designationCells[0, 0, 1].Position,
            designationCells[1, 0, 1].Position,
            designationCells[1, 0, 0].Position
        };
    }

    private Vector3 GetContentPosition()
    {
        Vector3 ret = Vector3.zero;
        foreach (DesignationCell cell in designationCells)
        {
            ret += cell.Position;
        }
        ret /= 8;
        return ret;
    }

    private DesignationCell[,,] GetDesignationCells()
    {
        DesignationCell[,,] ret = new DesignationCell[2, 2, 2];
        ret[0, 0, 0] = Quad.Points[0].DesignationCells[Height];
        ret[0, 0, 1] = Quad.Points[1].DesignationCells[Height];
        ret[1, 0, 1] = Quad.Points[2].DesignationCells[Height];
        ret[1, 0, 0] = Quad.Points[3].DesignationCells[Height];

        ret[0, 1, 0] = Quad.Points[0].DesignationCells[Height + 1];
        ret[0, 1, 1] = Quad.Points[1].DesignationCells[Height + 1];
        ret[1, 1, 1] = Quad.Points[2].DesignationCells[Height + 1];
        ret[1, 1, 0] = Quad.Points[3].DesignationCells[Height + 1];
        return ret;
    }

    public void InitializeNeighbors()
    {
        VisualCell up = GetUpNeighbor();
        VisualCell down = GetDownNeighbor();

        VisualCell left = GetAdjacentNeighbor(designationCells[0, 0, 0], designationCells[1, 0, 0]);
        VisualCell forward = GetAdjacentNeighbor(designationCells[0, 0, 0], designationCells[0, 0, 1]);

        VisualCell right = GetAdjacentNeighbor(designationCells[0, 0, 1], designationCells[1, 0, 1]);
        VisualCell back = GetAdjacentNeighbor(designationCells[1, 0, 0], designationCells[1, 0, 1]);

        Neighbors = new NeighborComponents(up, down, forward, back, left, right);
    }

    private VisualCell GetAdjacentNeighbor(DesignationCell cellA, DesignationCell cellB)
    {
        IEnumerable<GroundQuad> quads = cellA.GroundPoint.PolyConnections
            .Where(item => item.Points.Contains(cellB.GroundPoint));
        GroundQuad neighborQuad = quads.FirstOrDefault(item => item != Quad);
        if(neighborQuad == null)
        {
            return null;
        }
        return grid.GetVisualCell(neighborQuad, Height);
    }

    private VisualCell GetDownNeighbor()
    {
        if (Height == 0)
            return null;
        return grid.GetVisualCell(Quad, Height - 1);
    }

    private VisualCell GetUpNeighbor()
    {
        if (Height == MainGrid.MaxHeight - 2)
            return null;
        return grid.GetVisualCell(Quad, Height + 1);
    }

    public VoxelDesignation GetCurrentDesignation()
    {
        VoxelDesignation ret = new VoxelDesignation(new VoxelDesignationType[]{
            designationCells[0,0,0].Designation,
            designationCells[0,0,1].Designation,
            designationCells[0,1,0].Designation,
            designationCells[0,1,1].Designation,
            designationCells[1,0,0].Designation,
            designationCells[1,0,1].Designation,
            designationCells[1,1,0].Designation,
            designationCells[1,1,1].Designation,
        });
        SetAnyFills(ret.Description);
        return ret;
    }

    // If a column or top half of a designation is SlantedRoof of WalkableRoof, set it to AnyFill.
    private void SetAnyFills(VoxelDesignationType[,,] designation)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if(IsFill(designation[x, 1, z]))
                {
                    designation[x, 1, z] = VoxelDesignationType.AnyFilled;
                    if(IsFill(designation[x, 0, z]))
                    {
                        designation[x, 0, z] = VoxelDesignationType.AnyFilled;
                    }
                }
            }
        }
    }

    private bool IsFill(VoxelDesignationType slotType)
    {
        return slotType == VoxelDesignationType.SlantedRoof || slotType == VoxelDesignationType.WalkableRoof;
    }

    public void SetComponentTransform(Material mat)
    {
        if (Contents != null)
        {
            Vector3[] adjustedAnchors = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                int rotatedIndex = (i + 4 + Contents.Rotations) % 4;
                adjustedAnchors[i] = anchors[rotatedIndex];
            }
            if (Contents.Flipped)
            {
                Vector3 anchor0 = adjustedAnchors[0];
                Vector3 anchor2 = adjustedAnchors[2];
                adjustedAnchors[0] = adjustedAnchors[1];
                adjustedAnchors[1] = anchor0;
                adjustedAnchors[2] = adjustedAnchors[3];
                adjustedAnchors[3] = anchor2;
            }
            SetAnchors(adjustedAnchors, mat);
        }
        else
        {
            SetAnchors(anchors, mat);
        }
    }

    private void SetAnchors(Vector3[] adjustedAnchors, Material mat)
    {
        mat.SetVector("_AnchorA", adjustedAnchors[0]);
        mat.SetVector("_AnchorB", adjustedAnchors[1]);
        mat.SetVector("_AnchorC", adjustedAnchors[2]);
        mat.SetVector("_AnchorD", adjustedAnchors[3]);
    }
}
