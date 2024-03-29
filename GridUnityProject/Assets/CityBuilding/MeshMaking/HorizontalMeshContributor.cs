﻿using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelVisuals;

namespace MeshMaking
{
    internal class HorizontalMeshContributor : IMeshContributor
    {
        public IEnumerable<IMeshBuilderPoint> Points { get; }

        public IEnumerable<MeshBuilderTriangle> Triangles { get; }

        /// <summary>
        /// For the ground plane
        /// </summary>
        public HorizontalMeshContributor(GroundPoint groundPoint)
        {
            List<IMeshBuilderPoint> points = new List<IMeshBuilderPoint>();
            List<MeshBuilderTriangle> triangles = new List<MeshBuilderTriangle>();
            DesignationCell baseCell = groundPoint.DesignationCells[0];
            Vector3 lookTarget = Vector3.up;
            IMeshBuilderPoint corePoint = new MeshBuilderCellPoint(baseCell);

            points.Add(corePoint);
            foreach (GroundQuad quad in groundPoint.PolyConnections)
            {
                GroundPoint diagonal = quad.GetDiagonalPoint(groundPoint);
                Vector3 quadPos = new Vector3(quad.Center.x, 0, quad.Center.y);
                IMeshBuilderPoint diagonalPoint = new MeshBuilderConnectionPoint(baseCell, diagonal.DesignationCells[0], quadPos);
                GroundPoint[] otherPoints = quad.Points.Where(item => item != baseCell.GroundPoint && item != diagonal).ToArray();
                IMeshBuilderPoint otherPointA = new MeshBuilderConnectionPoint(baseCell, otherPoints[0].DesignationCells[0]);
                IMeshBuilderPoint otherPointB = new MeshBuilderConnectionPoint(baseCell, otherPoints[1].DesignationCells[0]);

                MeshBuilderTriangle triangleA = new MeshBuilderTriangle(baseCell, null, corePoint, diagonalPoint, otherPointA, lookTarget);
                MeshBuilderTriangle triangleB = new MeshBuilderTriangle(baseCell, null, diagonalPoint, corePoint, otherPointB, lookTarget);

                points.Add(diagonalPoint);
                points.Add(otherPointA);
                points.Add(otherPointB);
                triangles.Add(triangleA);
                triangles.Add(triangleB);
            }

            Points = points.ToArray();
            Triangles = triangles.ToArray();
        }

        public HorizontalMeshContributor(DesignationCell sourceCell, bool upwardFacing)
        {
            DesignationCell targetCell = upwardFacing ? sourceCell.CellAbove : sourceCell.CellBelow;
            DesignationCell alignmentCell = upwardFacing ? sourceCell.CellAbove : sourceCell;

            Vector3 lookTarget = upwardFacing ? Vector3.up : Vector3.down;

            List<IMeshBuilderPoint> points = new List<IMeshBuilderPoint>();
            List<MeshBuilderTriangle> triangles = new List<MeshBuilderTriangle>();

            IMeshBuilderPoint corePoint = new MeshBuilderCellPoint(alignmentCell);

            points.Add(corePoint);
            foreach (GroundQuad quad in targetCell.GroundPoint.PolyConnections)
            {
                GroundPoint diagonal = quad.GetDiagonalPoint(targetCell.GroundPoint);
                Vector3 quadPos = new Vector3(quad.Center.x, alignmentCell.Height, quad.Center.y);

                IMeshBuilderPoint diagonalPoint = new MeshBuilderConnectionPoint(alignmentCell, diagonal.DesignationCells[alignmentCell.Height], quadPos);
                GroundPoint[] otherPoints = quad.Points.Where(item => item != targetCell.GroundPoint && item != diagonal).ToArray();
                IMeshBuilderPoint otherPointA = new MeshBuilderConnectionPoint(alignmentCell, otherPoints[0].DesignationCells[alignmentCell.Height]);
                IMeshBuilderPoint otherPointB = new MeshBuilderConnectionPoint(alignmentCell, otherPoints[1].DesignationCells[alignmentCell.Height]);

                MeshBuilderTriangle triangleA = new MeshBuilderTriangle(targetCell, sourceCell, corePoint, diagonalPoint, otherPointA, lookTarget);
                MeshBuilderTriangle triangleB = new MeshBuilderTriangle(targetCell, sourceCell, diagonalPoint, corePoint, otherPointB, lookTarget);

                points.Add(diagonalPoint);
                points.Add(otherPointA);
                points.Add(otherPointB);
                triangles.Add(triangleA);
                triangles.Add(triangleB);
            }

            Points = points.ToArray();
            Triangles = triangles.ToArray();
        }
    }
}