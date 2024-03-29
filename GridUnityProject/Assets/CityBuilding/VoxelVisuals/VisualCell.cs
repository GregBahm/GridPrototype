﻿using System;
using System.Collections.Generic;
using System.Linq;
using GameGrid;
using UnityEngine;

namespace VoxelVisuals
{
    public class VisualCell
    {
        private static VisualCellOption defaultEmptyContents = GetDefaultEmptyContents();

        private static VisualCellOption GetDefaultEmptyContents()
        {
            Designation[] empty = new Designation[]
            {
                Designation.Empty,
                Designation.Empty,
                Designation.Empty,
                Designation.Empty,
                Designation.Empty,
                Designation.Empty,
                Designation.Empty,
                Designation.Empty,
            };
            VoxelVisualDesignation emptyDesignation = new VoxelVisualDesignation(empty);
            return new VisualCellOption(new ComponentInSet[0], emptyDesignation, VoxelConnectionType.None, VoxelConnectionType.None);
        }

        private readonly MainGrid grid;

        private VisualCellOption contents;
        public VisualCellOption Contents
        {
            get => contents;
            set
            {
                if (value != contents)
                {
                    VisualCellOption oldOption = contents;
                    contents = value;
                    var handler = ContentsChanged;
                    VisualCellChangedEventArg args = new VisualCellChangedEventArg(this, oldOption);
                    handler?.Invoke(this, args);
                }
            }
        }

        public GroundQuad Quad { get; }

        private readonly IDesignationCell[,,] designationCells;

        public NeighborComponents Neighbors { get; private set; }

        public int Height { get; }

        public static event EventHandler<VisualCellChangedEventArg> ContentsChanged;

        public VisualCell(MainGrid grid, GroundQuad quad, int height)
        {
            this.grid = grid;
            Quad = quad;
            Height = height;
            designationCells = GetDesignationCells();
            contents = defaultEmptyContents;
        }

        public Vector3 GetCenter()
        {
            Vector3 ret = Vector3.zero;
            foreach (Vector3 anchor in GetAnchors())
            {
                ret += anchor;
            }
            ret /= 8;
            return ret;
        }

        private Vector3[] GetAnchors()
        {
            return new Vector3[]
               {
            designationCells[1, 1, 0].Position,
            designationCells[0, 1, 0].Position,
            designationCells[0, 1, 1].Position,
            designationCells[1, 1, 1].Position,
               };
        }

        private IDesignationCell[,,] GetDesignationCells()
        {
            IDesignationCell[,,] ret = new IDesignationCell[2, 2, 2];
            if (Height == 0)
            {
                ret[0, 0, 0] = new ShellDesignationCell(Quad.Points[0]);
                ret[0, 0, 1] = new ShellDesignationCell(Quad.Points[1]);
                ret[1, 0, 1] = new ShellDesignationCell(Quad.Points[2]);
                ret[1, 0, 0] = new ShellDesignationCell(Quad.Points[3]);
            }
            else
            {
                ret[0, 0, 0] = Quad.Points[0].DesignationCells[Height - 1];
                ret[0, 0, 1] = Quad.Points[1].DesignationCells[Height - 1];
                ret[1, 0, 1] = Quad.Points[2].DesignationCells[Height - 1];
                ret[1, 0, 0] = Quad.Points[3].DesignationCells[Height - 1];
            }

            ret[0, 1, 0] = Quad.Points[0].DesignationCells[Height];
            ret[0, 1, 1] = Quad.Points[1].DesignationCells[Height];
            ret[1, 1, 1] = Quad.Points[2].DesignationCells[Height];
            ret[1, 1, 0] = Quad.Points[3].DesignationCells[Height];
            return ret;
        }

        public void InitializeNeighbors()
        {
            VisualCell up = GetUpNeighbor();
            VisualCell down = GetDownNeighbor();

            VisualCell left = GetAdjacentNeighbor(designationCells[0, 1, 0], designationCells[1, 1, 0]);
            VisualCell forward = GetAdjacentNeighbor(designationCells[0, 1, 0], designationCells[0, 1, 1]);

            VisualCell right = GetAdjacentNeighbor(designationCells[0, 1, 1], designationCells[1, 1, 1]);
            VisualCell back = GetAdjacentNeighbor(designationCells[1, 1, 0], designationCells[1, 1, 1]);

            Neighbors = new NeighborComponents(up, down, forward, back, left, right);
        }

        private VisualCell GetAdjacentNeighbor(IDesignationCell cellA, IDesignationCell cellB)
        {
            IEnumerable<GroundQuad> quads = cellA.GroundPoint.PolyConnections
                .Where(item => item.Points.Contains(cellB.GroundPoint));
            GroundQuad neighborQuad = quads.FirstOrDefault(item => item != Quad);
            if (neighborQuad == null)
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
            if (Height > grid.MaxHeight - 2)
                return null;
            return grid.GetVisualCell(Quad, Height + 1);
        }

        public VoxelVisualDesignation GetCurrentDesignation()
        {
            VoxelVisualDesignation ret = new VoxelVisualDesignation(new Designation[]{
                designationCells[0,0,0].Designation,
                designationCells[0,0,1].Designation,
                designationCells[0,1,0].Designation,
                designationCells[0,1,1].Designation,
                designationCells[1,0,0].Designation,
                designationCells[1,0,1].Designation,
                designationCells[1,1,0].Designation,
                designationCells[1,1,1].Designation,
            });
            return ret;
        }

        private Vector3[] GetAdjustedAnchors(ComponentInSet component)
        {
            Vector3[] anchors = GetAnchors();
            Vector3[] ret = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                int rotatedIndex = (i + 4 + component.Rotations) % 4;
                ret[i] = anchors[rotatedIndex];
            }
            if (component.Flipped)
            {
                Vector3 anchor0 = ret[0];
                Vector3 anchor2 = ret[2];
                ret[0] = ret[1];
                ret[1] = anchor0;
                ret[2] = ret[3];
                ret[3] = anchor2;
            }
            return ret;
        }

        public VoxelRenderData GetRenderData(ComponentInSet component)
        {
            Vector3[] adjustedAnchors = GetAdjustedAnchors(component);
            float flipNormal = component.Flipped ? -1 : 1;
            return new VoxelRenderData(
                new Vector2(adjustedAnchors[0].x, adjustedAnchors[0].z),
                new Vector2(adjustedAnchors[1].x, adjustedAnchors[1].z),
                new Vector2(adjustedAnchors[2].x, adjustedAnchors[2].z),
                new Vector2(adjustedAnchors[3].x, adjustedAnchors[3].z),
                Height,
                flipNormal);
        }
    }
}