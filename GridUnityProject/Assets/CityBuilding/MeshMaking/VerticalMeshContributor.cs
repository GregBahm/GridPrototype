using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
    internal class VerticalMeshContributor : IMeshContributor
    {
        private readonly List<IMeshBuilderPoint> points = new List<IMeshBuilderPoint>();
        public IEnumerable<IMeshBuilderPoint> Points { get { return points; } }
        private readonly List<MeshBuilderTriangle> triangles = new List<MeshBuilderTriangle>();
        public IEnumerable<MeshBuilderTriangle> Triangles { get { return triangles; } }

        public VerticalMeshContributor(DesignationCell sourceCell, GroundEdge edge, DesignationCell connectedCell)
        {
            DesignationCell baseAbove = sourceCell.CellAbove;
            DesignationCell connectedAbove = connectedCell.CellAbove;
            MeshBuilderConnectionPoint edgePoint = new MeshBuilderConnectionPoint(sourceCell, connectedCell);
            MeshBuilderConnectionPoint edgeAbovePoint = new MeshBuilderConnectionPoint(baseAbove, connectedAbove);

            if(!connectedCell.IsFilled || edge.IsBorder)
            {
                points.Add(edgePoint);
                points.Add(edgeAbovePoint);
            }

            if (!connectedCell.IsFilled)
            {
                foreach (GroundQuad quad in edge.Quads)
                {
                    Vector3 quadPos = new Vector3(quad.Center.x, sourceCell.Height, quad.Center.y);
                    DesignationCell diagonalCell = quad.GetDiagonalPoint(sourceCell.GroundPoint).DesignationCells[sourceCell.Height];
                    MeshBuilderConnectionPoint diagonalPoint = new MeshBuilderConnectionPoint(diagonalCell, sourceCell, quadPos);
                    DesignationCell diagonalAbove = diagonalCell.CellAbove;
                    Vector3 quadAbove = new Vector3(quad.Center.x, sourceCell.Height + 1, quad.Center.y);
                    MeshBuilderConnectionPoint diagonalAbovePoint = new MeshBuilderConnectionPoint(diagonalAbove, baseAbove, quadAbove);

                    points.Add(diagonalPoint);
                    points.Add(diagonalAbovePoint);

                    MeshBuilderTriangle triA = new MeshBuilderTriangle(connectedCell, sourceCell, edgePoint, diagonalPoint, diagonalAbovePoint);
                    MeshBuilderTriangle triB = new MeshBuilderTriangle(connectedCell, sourceCell, edgePoint, diagonalAbovePoint, edgeAbovePoint);
                    triangles.Add(triA);
                    triangles.Add(triB);
                }
            }

            if (edge.IsBorder)
            {
                MeshBuilderCellPoint basePoint = new MeshBuilderCellPoint(sourceCell);
                MeshBuilderCellPoint abovePoint = new MeshBuilderCellPoint(baseAbove);
                points.Add(basePoint);
                points.Add(abovePoint);

                Vector2 quadCenter = edge.Quads.First().Center;
                Vector3 quadPos = new Vector3(quadCenter.x, sourceCell.Height, quadCenter.y);
                Vector3 lookTarget = edgePoint.Position - quadPos;
                MeshBuilderTriangle borderA = new MeshBuilderTriangle(null, sourceCell, basePoint, abovePoint, edgePoint, lookTarget);
                MeshBuilderTriangle borderB = new MeshBuilderTriangle(null, sourceCell, abovePoint, edgeAbovePoint, edgePoint, lookTarget);
                triangles.Add(borderA);
                triangles.Add(borderB);
            }
        }
    }
}