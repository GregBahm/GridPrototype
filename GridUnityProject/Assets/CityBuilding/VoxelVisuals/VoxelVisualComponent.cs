using System;
using System.Collections.Generic;
using System.Linq;
using GameGrid;
using UnityEngine;

public class VoxelVisualComponent
{
    public VoxelCell Core { get; }
    public GroundQuad Quad { get; }
    public bool OnTopHalf { get; }
    private readonly VoxelVisualsLayer bottomLayer;
    private readonly VoxelVisualsLayer topLayer;

    public VoxelVisualOption Contents { get; set; }
    public Vector3 ContentPosition { get; }
    public Vector3 VisualCenter
    {
        get
        {
            return ((anchors[0] + anchors[1] + anchors[2] + anchors[3]) / 4) + ContentPosition;
        }
    }

    public NeighborComponents Neighbors { get; private set; }

    public int VisualsIndex { get; }

    private Vector3[] anchors;

    private bool isOnGround;

    public VoxelVisualComponent(VoxelCell coreCell, GroundQuad quad, bool onTopHalf, int visualsIndex)
    {
        Core = coreCell;
        Quad = quad;
        OnTopHalf = onTopHalf;
        isOnGround = coreCell.CellBelow == null;
        VoxelCell bottomCell = onTopHalf ? coreCell : (coreCell.CellBelow ?? coreCell);
        VoxelCell topCell = onTopHalf ? (coreCell.CellAbove ?? coreCell) : coreCell;
        bottomLayer = new VoxelVisualsLayer(bottomCell, quad);
        topLayer = new VoxelVisualsLayer(topCell, quad);
        ContentPosition = coreCell.CellPosition + (onTopHalf ? new Vector3(0, .5f, 0) : Vector3.zero);
        VisualsIndex = visualsIndex;
    }

    public void InitializeNeighbors()
    {
        VoxelVisualComponent up = GetUpNeighbor();
        VoxelVisualComponent down = GetDownNeighbor();

        VoxelVisualComponent left = Core.Visuals.GetLeftNeighbor(this);
        VoxelVisualComponent forward = Core.Visuals.GetForwardNeighbor(this);

        VoxelVisualComponent right = GetHorizontalNeighbor(bottomLayer.AdjacentCellB);
        VoxelVisualComponent back = GetHorizontalNeighbor(bottomLayer.AdjacentCellA);

        Neighbors = new NeighborComponents(up, down, forward, back, left, right);
        anchors = GetAnchors();
    }

    private Vector3[] GetAnchors()
    {
        Vector3 anchorA = (bottomLayer.AdjacentCellA.CellPosition - bottomLayer.BasisCell.CellPosition) / 2;
        Vector3 anchorB = Vector3.zero;
        Vector3 anchorC = (bottomLayer.AdjacentCellB.CellPosition - bottomLayer.BasisCell.CellPosition) / 2;
        Vector3 anchorD = bottomLayer.Center - bottomLayer.BasisCell.CellPosition;
        return new Vector3[] { anchorA, anchorB, anchorC, anchorD };
    }

    private VoxelVisualComponent GetHorizontalNeighbor(VoxelCell perpendicularCell)
    {
        if (perpendicularCell == null)
            return null;
        return perpendicularCell.Visuals.GetComponent(Quad, OnTopHalf);
    }

    private VoxelVisualComponent GetDownNeighbor()
    {
        if (OnTopHalf)
        {
            return Core.Visuals.GetComponent(Quad, false);
        }
        if (isOnGround)
        {
            return null;
        }
        return Core.CellBelow.Visuals.GetComponent(Quad, true);
    }

    private VoxelVisualComponent GetUpNeighbor()
    {
        if (OnTopHalf)
        {
            if (Core.CellAbove == null)
            {
                return null;
            }
            return Core.CellAbove.Visuals.GetComponent(Quad, false);
        }
        return Core.Visuals.GetComponent(Quad, true);
    }

    public VoxelDesignation GetCurrentDesignation()
    {
        VoxelDesignationType[,] bottomDesignationLayer = GetDesignationLayer(bottomLayer);
        VoxelDesignationType[,] topDesignationLayer = GetDesignationLayer(topLayer);
        VoxelDesignation designation = new VoxelDesignation();
        if (OnTopHalf)
        {
            // bottom goes straignt in, top is AND
            designation.Description[0, 0, 0] = bottomDesignationLayer[0, 0];
            designation.Description[0, 0, 1] = bottomDesignationLayer[0, 1];
            designation.Description[1, 0, 0] = bottomDesignationLayer[1, 0];
            designation.Description[1, 0, 1] = bottomDesignationLayer[1, 1];

            designation.Description[0, 1, 0] = GetConnectedDesignation(bottomDesignationLayer[0, 0], topDesignationLayer[0, 0]);
            designation.Description[0, 1, 1] = GetConnectedDesignation(bottomDesignationLayer[0, 1], topDesignationLayer[0, 1]);
            designation.Description[1, 1, 0] = GetConnectedDesignation(bottomDesignationLayer[1, 0], topDesignationLayer[1, 0]);
            designation.Description[1, 1, 1] = GetConnectedDesignation(bottomDesignationLayer[1, 1], topDesignationLayer[1, 1]);
        }
        else
        {
            if (isOnGround)
            {
                designation.Description[0, 0, 0] = VoxelDesignationType.Ground;
                designation.Description[0, 0, 1] = VoxelDesignationType.Ground;
                designation.Description[1, 0, 0] = VoxelDesignationType.Ground;
                designation.Description[1, 0, 1] = VoxelDesignationType.Ground;
            }
            else
            {
                // top goes straignt in, bottom is AND
                designation.Description[0, 0, 0] = GetConnectedDesignation(bottomDesignationLayer[0, 0], topDesignationLayer[0, 0]);
                designation.Description[0, 0, 1] = GetConnectedDesignation(bottomDesignationLayer[0, 1], topDesignationLayer[0, 1]);
                designation.Description[1, 0, 0] = GetConnectedDesignation(bottomDesignationLayer[1, 0], topDesignationLayer[1, 0]);
                designation.Description[1, 0, 1] = GetConnectedDesignation(bottomDesignationLayer[1, 1], topDesignationLayer[1, 1]);
            }

            designation.Description[0, 1, 0] = topDesignationLayer[0, 0];
            designation.Description[0, 1, 1] = topDesignationLayer[0, 1];
            designation.Description[1, 1, 0] = topDesignationLayer[1, 0];
            designation.Description[1, 1, 1] = topDesignationLayer[1, 1];
        }
        SetAnyFills(designation.Description);
        return designation;
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

    // In the case of a mismatch, currently defaults to walkable
    private static VoxelDesignationType GetConnectedDesignation(params VoxelDesignationType[] designations)
    {
        if (designations.Any(item => item == VoxelDesignationType.Empty))
            return VoxelDesignationType.Empty;
        if (designations.All(item => item == designations[0]))
            return designations[0];
        return VoxelDesignationType.WalkableRoof;
    }

    private VoxelDesignationType[,] GetDesignationLayer(VoxelVisualsLayer bottomLayer)
    {
        VoxelDesignationType[,] ret = new VoxelDesignationType[2, 2];
        ret[1, 0] = GetConnectedDesignation(bottomLayer.BasisCell.Designation, bottomLayer.AdjacentCellA.Designation);
        ret[0, 0] = bottomLayer.BasisCell.Designation;
        ret[0, 1] = GetConnectedDesignation(bottomLayer.BasisCell.Designation, bottomLayer.AdjacentCellB.Designation);
        ret[1, 1] = GetConnectedDesignation(bottomLayer.BasisCell.Designation,
            bottomLayer.DiagonalCell.Designation,
            bottomLayer.AdjacentCellA.Designation,
            bottomLayer.AdjacentCellB.Designation);
        return ret;
    }

    public void SetComponentTransform(Material mat)
    {
        if (Contents != null)
        {
            Vector3[] rotatedAnchors = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                int rotatedIndex = (i + 4 + Contents.Rotations) % 4;
                rotatedAnchors[i] = anchors[rotatedIndex];
            }
            if (Contents.Flipped)
            {
                Vector3 anchor0 = rotatedAnchors[0];
                Vector3 anchor2 = rotatedAnchors[2];
                rotatedAnchors[0] = rotatedAnchors[1];
                rotatedAnchors[1] = anchor0;
                rotatedAnchors[2] = rotatedAnchors[3];
                rotatedAnchors[3] = anchor2;
            }
            SetAnchors(rotatedAnchors, mat);
        }
        else
        {
            SetAnchors(anchors, mat);
        }
    }

    private void SetAnchors(Vector3[] anchors, Material mat)
    {
        mat.SetVector("_AnchorA", anchors[0]);
        mat.SetVector("_AnchorB", anchors[1]);
        mat.SetVector("_AnchorC", anchors[2]);
        mat.SetVector("_AnchorD", anchors[3]);
    }

    private class VoxelVisualsLayer
    {
        public VoxelCell BasisCell { get; }
        public GroundQuad BasisQuad { get; }
        public VoxelCell AdjacentCellA { get; }
        public VoxelCell AdjacentCellB { get; }
        public VoxelCell DiagonalCell { get; }
        public Vector3 Center { get; }

        public VoxelVisualsLayer(VoxelCell basisCell, GroundQuad basisQuad)
        {
            BasisCell = basisCell;
            BasisQuad = basisQuad;
            Center = new Vector3(basisQuad.Center.x, basisCell.Height, basisQuad.Center.y);

            GroundPoint diagonalPoint = basisQuad.GetDiagonalPoint(basisCell.GroundPoint);
            DiagonalCell = diagonalPoint.Voxels[basisCell.Height];

            GroundPoint[] otherPoints = basisQuad.Points.Where(item => item != basisCell.GroundPoint && item != diagonalPoint).ToArray();
            AdjacentCellA = otherPoints[0].Voxels[basisCell.Height];
            AdjacentCellB = otherPoints[1].Voxels[basisCell.Height];
        }
    }

    public override string ToString()
    {
        return "Component in " + Core.ToString() + ", " + (OnTopHalf ? " top" : " bottom") + " half";
    }
}
