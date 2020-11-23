using System;
using System.Collections.Generic;
using System.Linq;
using GameGrid;
using UnityEngine;

public class VoxelVisuals
{
    public VoxelCell Cell { get; }

    public IEnumerable<VoxelVisualComponent> Components { get; }

    public VoxelVisuals(VoxelCell cell)
    {
        Cell = cell;
        Components = CreateVisualComponents().ToArray();
    }

    private IEnumerable<VoxelVisualComponent> CreateVisualComponents()
    {
        foreach (GroundQuad quad in Cell.GroundPoint.PolyConnections)
        {
            yield return new VoxelVisualComponent(Cell, quad);
        }
    }
}


public class VoxelVisualComponent
{
    public VoxelCell Core { get; }
    public GroundQuad Quad { get; }
    private readonly VoxelVisualsLayer bottomLayer;
    private readonly VoxelVisualsLayer topLayer;

    public VoxelVisualOption Contents { get; set; }
    public Vector3 ContentPosition { get; }

    public VoxelVisualComponent(VoxelCell coreCell, GroundQuad quad)
    {
        Core = coreCell;
        Quad = quad;
        bottomLayer = new VoxelVisualsLayer(coreCell, quad);
        topLayer = new VoxelVisualsLayer(coreCell.CellAbove, quad);
        ContentPosition = coreCell.CellPosition;
    }

    public VoxelDesignation GetCurrentDesignation()
    {
        VoxelDesignation designation = new VoxelDesignation();
        designation.Description[0, 0, 0] = bottomLayer.BasisCell.Filled;
        designation.Description[0, 1, 0] = topLayer.BasisCell.Filled;

        // Adjacent values are only filled if both the core and the adjacent values are filled
        designation.Description[1, 0, 0] = bottomLayer.BasisCell.Filled && bottomLayer.AdjacentCellA.Filled;
        designation.Description[1, 1, 0] = topLayer.BasisCell.Filled && topLayer.AdjacentCellA.Filled;
        designation.Description[0, 0, 1] = bottomLayer.BasisCell.Filled && bottomLayer.AdjacentCellB.Filled;
        designation.Description[0, 1, 1] = topLayer.BasisCell.Filled && topLayer.AdjacentCellB.Filled;

        designation.Description[1, 0, 1] = bottomLayer.BasisCell.Filled && bottomLayer.DiagonalCell.Filled;
        designation.Description[1, 1, 1] = topLayer.BasisCell.Filled && topLayer.DiagonalCell.Filled;
        return designation;
    }

    private class VoxelVisualsLayer
    {
        public VoxelCell BasisCell { get; }
        public VoxelCell AdjacentCellA { get; }
        public VoxelCell AdjacentCellB { get; }
        public VoxelCell DiagonalCell { get; }

        public VoxelVisualsLayer(VoxelCell basisCell, GroundQuad quad)
        {
            BasisCell = basisCell;

            GroundPoint diagonalPoint = quad.GetDiagonalPoint(basisCell.GroundPoint);
            DiagonalCell = diagonalPoint.Voxels[basisCell.Height];

            GroundPoint[] otherPoints = quad.Points.Where(item => item != basisCell.GroundPoint && item != diagonalPoint).ToArray();
            AdjacentCellA = otherPoints[0].Voxels[basisCell.Height];
            AdjacentCellB = otherPoints[1].Voxels[basisCell.Height];
        }
    }

    internal void SetComponentTransform(Material mat)
    {
        Vector3 anchorB = (bottomLayer.AdjacentCellA.CellPosition - bottomLayer.BasisCell.CellPosition) / 2;
        Vector3 anchorC = (bottomLayer.DiagonalCell.CellPosition - bottomLayer.BasisCell.CellPosition) / 2;
        Vector3 anchorD = (bottomLayer.AdjacentCellB.CellPosition - bottomLayer.BasisCell.CellPosition) / 2;
        mat.SetVector("_AnchorB", anchorB);
        mat.SetVector("_AnchorC", anchorC);
        mat.SetVector("_AnchorD", anchorD);
    }
}
