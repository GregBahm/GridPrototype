using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            VoxelCell baseCell = groundPoint.Voxels[0];
            Vector3 lookTarget = Vector3.up;
            IMeshBuilderPoint corePoint = new MeshBuilderCellPoint(baseCell);

            points.Add(corePoint);
            foreach (GroundQuad quad in groundPoint.PolyConnections)
            {
                GroundPoint diagonal = quad.GetDiagonalPoint(groundPoint);
                Vector3 quadPos = new Vector3(quad.Center.x, 0, quad.Center.y);
                IMeshBuilderPoint diagonalPoint = new MeshBuilderConnectionPoint(baseCell, diagonal.Voxels[0], quadPos);
                GroundPoint[] otherPoints = quad.Points.Where(item => item != baseCell.GroundPoint && item != diagonal).ToArray();
                IMeshBuilderPoint otherPointA = new MeshBuilderConnectionPoint(baseCell, otherPoints[0].Voxels[0]);
                IMeshBuilderPoint otherPointB = new MeshBuilderConnectionPoint(baseCell, otherPoints[1].Voxels[0]);

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

        public HorizontalMeshContributor(VoxelCell sourceCell, bool upwardFacing)
        {
            VoxelCell targetCell = upwardFacing ? sourceCell.CellAbove : sourceCell.CellBelow;
            VoxelCell alignmentCell = upwardFacing ? sourceCell.CellAbove : sourceCell;

            Vector3 lookTarget = upwardFacing ? Vector3.up : Vector3.down;

            List<IMeshBuilderPoint> points = new List<IMeshBuilderPoint>();
            List<MeshBuilderTriangle> triangles = new List<MeshBuilderTriangle>();

            IMeshBuilderPoint corePoint = new MeshBuilderCellPoint(alignmentCell);

            points.Add(corePoint);
            foreach (GroundQuad quad in targetCell.GroundPoint.PolyConnections)
            {
                GroundPoint diagonal = quad.GetDiagonalPoint(targetCell.GroundPoint);
                Vector3 quadPos = new Vector3(quad.Center.x, alignmentCell.Height, quad.Center.y);

                IMeshBuilderPoint diagonalPoint = new MeshBuilderConnectionPoint(alignmentCell, diagonal.Voxels[alignmentCell.Height], quadPos);
                GroundPoint[] otherPoints = quad.Points.Where(item => item != targetCell.GroundPoint && item != diagonal).ToArray();
                IMeshBuilderPoint otherPointA = new MeshBuilderConnectionPoint(alignmentCell, otherPoints[0].Voxels[alignmentCell.Height]);
                IMeshBuilderPoint otherPointB = new MeshBuilderConnectionPoint(alignmentCell, otherPoints[1].Voxels[alignmentCell.Height]);

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