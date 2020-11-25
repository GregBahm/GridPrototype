﻿using System.Linq;
using GameGrid;
using UnityEngine;

public class VoxelVisualComponent
{
    public VoxelCell Core { get; }
    public GroundQuad Quad { get; }
    public bool TopHalf { get; }
    private readonly VoxelVisualsLayer bottomLayer;
    private readonly VoxelVisualsLayer topLayer;

    public VoxelVisualOption Contents { get; set; }
    public Vector3 ContentPosition { get; }

    public VoxelVisualComponent(VoxelCell coreCell, GroundQuad quad, bool topHalf)
    {
        Core = coreCell;
        Quad = quad;
        TopHalf = topHalf;
        VoxelCell bottomCell = topHalf ? coreCell : (coreCell.CellBelow ?? coreCell);
        VoxelCell topCell = topHalf ? (coreCell.CellAbove ?? coreCell) : coreCell;
        bottomLayer = new VoxelVisualsLayer(bottomCell, quad);
        topLayer = new VoxelVisualsLayer(topCell, quad);
        ContentPosition = coreCell.CellPosition + (topHalf ? new Vector3(0, .5f, 0) : Vector3.zero);
    }

    public VoxelDesignation GetCurrentDesignation()
    {
        bool[,] bottomDesignationLayer = GetDesignationLayer(bottomLayer);
        bool[,] topDesignationLayer = GetDesignationLayer(topLayer);
        VoxelDesignation designation = new VoxelDesignation();
        if(TopHalf)
        {
            // bottom goes straignt in, top is AND
            designation.Description[0, 0, 0] = bottomDesignationLayer[0, 0];
            designation.Description[1, 0, 0] = bottomDesignationLayer[1, 0];
            designation.Description[0, 0, 1] = bottomDesignationLayer[0, 1];
            designation.Description[1, 0, 1] = bottomDesignationLayer[1, 1];

            designation.Description[0, 1, 0] = bottomDesignationLayer[0, 0] && topDesignationLayer[0,0];
            designation.Description[1, 1, 0] = bottomDesignationLayer[1, 0] && topDesignationLayer[1, 0];
            designation.Description[0, 1, 1] = bottomDesignationLayer[0, 1] && topDesignationLayer[0, 1];
            designation.Description[1, 1, 1] = bottomDesignationLayer[1, 1] && topDesignationLayer[1, 1];
        }
        else
        {
            // bottom goes straignt in, top is AND
            designation.Description[0, 0, 0] = bottomDesignationLayer[0, 0] && topDesignationLayer[0, 0];
            designation.Description[1, 0, 0] = bottomDesignationLayer[1, 0] && topDesignationLayer[1, 0];
            designation.Description[0, 0, 1] = bottomDesignationLayer[0, 1] && topDesignationLayer[0, 1];
            designation.Description[1, 0, 1] = bottomDesignationLayer[1, 1] && topDesignationLayer[1, 1];

            designation.Description[0, 1, 0] = topDesignationLayer[0, 0]; 
            designation.Description[1, 1, 0] = topDesignationLayer[1, 0]; 
            designation.Description[0, 1, 1] = topDesignationLayer[0, 1];
            designation.Description[1, 1, 1] = topDesignationLayer[1, 1]; 
        }
        return designation;
    }

    private bool[,] GetDesignationLayer(VoxelVisualsLayer bottomLayer)
    {
        bool[,] ret = new bool[2, 2];
        ret[0,0] = bottomLayer.BasisCell.Filled;
        ret[1,0] = bottomLayer.BasisCell.Filled && bottomLayer.AdjacentCellA.Filled;
        ret[0,1] = bottomLayer.BasisCell.Filled && bottomLayer.AdjacentCellB.Filled;
        ret[1,1] = bottomLayer.BasisCell.Filled
            && bottomLayer.DiagonalCell.Filled
            && bottomLayer.AdjacentCellA.Filled
            && bottomLayer.AdjacentCellB.Filled;
        return ret;
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

    internal void SetComponentTransform(Material mat)
    {
        Vector3 anchorA = Vector3.zero;
        Vector3 anchorB = (bottomLayer.AdjacentCellA.CellPosition - bottomLayer.BasisCell.CellPosition) / 2;
        Vector3 anchorC = bottomLayer.Center - bottomLayer.BasisCell.CellPosition;
        Vector3 anchorD = (bottomLayer.AdjacentCellB.CellPosition - bottomLayer.BasisCell.CellPosition) / 2;
        Vector3[] baseAnchors = new Vector3[] { anchorA, anchorB, anchorC, anchorD };
        if (Contents != null)
        {
            Vector3[] rotatedAnchors = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                int rotatedIndex = (i + 4 - Contents.Rotations) % 4;
                rotatedAnchors[i] = baseAnchors[rotatedIndex];
            }
            if(Contents.Flipped)
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
            SetAnchors(baseAnchors, mat);
        }
    }

    private void SetAnchors(Vector3[] anchors, Material mat)
    {

        mat.SetVector("_AnchorA", anchors[0]);
        mat.SetVector("_AnchorB", anchors[1]);
        mat.SetVector("_AnchorC", anchors[2]);
        mat.SetVector("_AnchorD", anchors[3]);
    }
}
