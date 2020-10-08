using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

public class InteractionMesh
{
    private readonly List<IHitTarget> hitTable = new List<IHitTarget>();

    public Mesh Mesh { get; }

    public InteractionMesh(Mesh mesh)
    {
        Mesh = mesh;
    }
    
    public IHitTarget GetHitTarget(int hitTringleIndex)
    {
        return hitTable[hitTringleIndex];
    }

    public void UpdateMesh(MainGrid grid)
    {
        IEnumerable<IMeshContributor> meshContributors = GetMeshContributors(grid).ToArray();
        VertTable vertTable = new VertTable(meshContributors);

        Mesh.Clear();
        Mesh.vertices = vertTable.GetPoints();

        hitTable.Clear();
        List<int> triangles = new List<int>();
        foreach (IMeshContributor item in meshContributors)
        {
            foreach (MeshBuilderTriangle triangle in item.Triangles)
            {
                triangles.Add(vertTable.GetVertIndex(triangle.PointA));
                triangles.Add(vertTable.GetVertIndex(triangle.PointB));
                triangles.Add(vertTable.GetVertIndex(triangle.PointC));
                hitTable.Add(triangle);
            }
        }
        Mesh.triangles = triangles.ToArray();
    }

    private IEnumerable<IMeshContributor> GetMeshContributors(MainGrid grid)
    {
        IMeshContributor[] groundContributor = grid.Points.Where(item => !item.Voxels[0].Filled).Select(item => new HorizontalMeshContributor(item.Voxels[0], null)).ToArray();
        IMeshContributor[] contributors = grid.FilledCells.Select(item => new CellMeshContributor(item)).ToArray();
        return groundContributor.Concat(contributors);
    }

    private class MeshBuilderTriangle : IHitTarget
    {
        public IMeshBuilderPoint PointA { get; }
        public IMeshBuilderPoint PointB { get; }
        public IMeshBuilderPoint PointC { get; }

        public VoxelCell TargetCell { get; }
        public VoxelCell SourceCell { get; }

        public MeshBuilderTriangle(VoxelCell targetCell, 
            VoxelCell sourceCell, 
            IMeshBuilderPoint pointA, 
            IMeshBuilderPoint pointB, 
            IMeshBuilderPoint pointC)
        {
            TargetCell = targetCell;
            SourceCell = sourceCell;
            PointA = pointA;
            bool reorderVerts = sourceCell == null?
                GetShouldReorderVerts(pointA.Position, pointB.Position, pointC.Position) : // Is a basepoint
                GetShouldReorderVerts(targetCell, pointA.Position, pointB.Position, pointC.Position);
            PointB = reorderVerts ? pointB : pointC;
            PointC = reorderVerts ? pointC : pointB;
        }

        private bool GetShouldReorderVerts(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 crossVector = Vector3.Cross(pointA - pointB, pointA - pointC);
            return crossVector.y > 0;
        }

        private bool GetShouldReorderVerts(VoxelCell targetCell, Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 crossVector = Vector3.Cross(pointA - pointB, pointA - pointC);
            Vector3 toTarget = pointA - targetCell.CellPosition;
            return Vector3.Dot(toTarget, crossVector) < 0;
        }
    }

    private interface IMeshContributor
    {
        IEnumerable<IMeshBuilderPoint> Points { get; }

        IEnumerable<MeshBuilderTriangle> Triangles { get; }
    }

    private class VertTable
    {
        private readonly List<Vector3> pointPositions = new List<Vector3>();
        private readonly Dictionary<string, int> indexTable = new Dictionary<string, int>();

        public VertTable(IEnumerable<IMeshContributor> meshContributors)
        {
            foreach (IMeshBuilderPoint point in meshContributors.SelectMany(item => item.Points))
            {
                if(!indexTable.ContainsKey(point.Key))
                {
                    indexTable.Add(point.Key, pointPositions.Count);
                    pointPositions.Add(point.Position);
                }
            }
        }

        public Vector3[] GetPoints()
        {
            return pointPositions.ToArray();
        }

        public int GetVertIndex(IMeshBuilderPoint point)
        {
            return indexTable[point.Key];
        }
    }

    private class HorizontalMeshContributor : IMeshContributor
    {
        public IEnumerable<IMeshBuilderPoint> Points { get; }

        public IEnumerable<MeshBuilderTriangle> Triangles { get; }

        public HorizontalMeshContributor(VoxelCell baseCell, VoxelCell sourceCell)
        {
            List<IMeshBuilderPoint> points = new List<IMeshBuilderPoint>();
            List<MeshBuilderTriangle> triangles = new List<MeshBuilderTriangle>();

            IMeshBuilderPoint corePoint = new MeshBuilderCorePoint(baseCell);

            points.Add(corePoint);
            foreach (GroundQuad quad in baseCell.GroundPoint.PolyConnections)
            {
                GroundPoint diagonal = quad.GetDiagonalPoint(baseCell.GroundPoint);
                Vector3 quadPos = new Vector3(quad.Center.x, baseCell.Height, quad.Center.y);
                IMeshBuilderPoint diagonalPoint = new MeshBuilderSubPoint(baseCell, diagonal.Voxels[0], quadPos);
                GroundPoint[] otherPoints = quad.Points.Where(item => item != baseCell.GroundPoint && item != diagonal).ToArray();
                IMeshBuilderPoint otherPointA = new MeshBuilderSubPoint(baseCell, otherPoints[0].Voxels[0]);
                IMeshBuilderPoint otherPointB = new MeshBuilderSubPoint(baseCell, otherPoints[1].Voxels[0]);

                MeshBuilderTriangle triangleA = new MeshBuilderTriangle(baseCell, sourceCell, corePoint, diagonalPoint, otherPointA);
                MeshBuilderTriangle triangleB = new MeshBuilderTriangle(baseCell, sourceCell, diagonalPoint, corePoint, otherPointB);

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

    private class CellMeshContributor : IMeshContributor
    {
        private readonly VoxelCell cell;
        public IEnumerable<IMeshBuilderPoint> Points { get; }
        public IEnumerable<MeshBuilderTriangle> Triangles { get; }

        public CellMeshContributor(VoxelCell cell)
        {
            this.cell = cell;

            List<IMeshContributor> subContributors = new List<IMeshContributor>();
            if(GetDoesHaveBottom())
            {
                HorizontalMeshContributor groundContributor = new HorizontalMeshContributor(cell, cell.CellBelow);
                subContributors.Add(groundContributor);
            }
            if (GetDoesHaveTop())
            {
                HorizontalMeshContributor groundContributor = new HorizontalMeshContributor(cell, cell.CellAbove);
                subContributors.Add(groundContributor);
            }
            SideToFill[] sidesToFill = GetSidesToFill().ToArray();
            subContributors.AddRange(sidesToFill);
            Points = subContributors.SelectMany(item => item.Points).ToArray();
            Triangles = subContributors.SelectMany(item => item.Triangles).ToArray();
        }

        private IEnumerable<SideToFill> GetSidesToFill()
        {
            foreach (GroundEdge edge in cell.GroundPoint.Edges)
            {
                VoxelCell connectedCell = edge.GetOtherPoint(cell.GroundPoint).Voxels[cell.Height];
                if(!connectedCell.Filled)
                {
                    yield return new SideToFill(cell, edge, connectedCell);
                }
            }
        }

        private bool GetDoesHaveTop()
        {
            return (cell.Height == MainGrid.VoxelHeight - 1) || !cell.GroundPoint.Voxels[cell.Height + 1].Filled;
        }

        private bool GetDoesHaveBottom()
        {
            return cell.Height != 0 && !cell.GroundPoint.Voxels[cell.Height - 1].Filled;
        }

        private class SideToFill : IMeshContributor
        {
            private readonly List<MeshBuilderSubPoint> points = new List<MeshBuilderSubPoint>();
            public IEnumerable<IMeshBuilderPoint> Points { get { return points; } }
            private readonly List<MeshBuilderTriangle> triangles = new List<MeshBuilderTriangle>();
            public IEnumerable<MeshBuilderTriangle> Triangles { get { return triangles; } } 
            
            public SideToFill(VoxelCell sourceCell, GroundEdge edge, VoxelCell connectedCell)
            {
                foreach (GroundQuad quad in edge.Quads)
                {
                    Vector3 quadPos = new Vector3(quad.Center.x, sourceCell.Height, quad.Center.y);
                    MeshBuilderSubPoint edgePoint = new MeshBuilderSubPoint(sourceCell, connectedCell);
                    VoxelCell diagonalCell = quad.GetDiagonalPoint(sourceCell.GroundPoint).Voxels[sourceCell.Height];
                    MeshBuilderSubPoint diagonalPoint = new MeshBuilderSubPoint(diagonalCell, sourceCell, quadPos);
                    VoxelCell baseAbove = sourceCell.CellAbove;
                    VoxelCell connectedAbove = connectedCell.CellAbove;
                    VoxelCell diagonalAbove = diagonalCell.CellAbove;
                    Vector3 quadAbove = new Vector3(quad.Center.x, sourceCell.Height + 1, quad.Center.y);
                    MeshBuilderSubPoint edgeAbovePoint = new MeshBuilderSubPoint(baseAbove, connectedAbove);
                    MeshBuilderSubPoint diagonalAbovePoint = new MeshBuilderSubPoint(diagonalAbove, baseAbove, quadAbove);

                    points.Add(edgePoint);
                    points.Add(diagonalPoint);
                    points.Add(edgeAbovePoint);
                    points.Add(diagonalAbovePoint);

                    MeshBuilderTriangle triA = new MeshBuilderTriangle(connectedCell, sourceCell, edgePoint, diagonalPoint, diagonalAbovePoint);
                    MeshBuilderTriangle triB = new MeshBuilderTriangle(connectedCell, sourceCell, edgePoint, diagonalAbovePoint, edgeAbovePoint);
                    triangles.Add(triA);
                    triangles.Add(triB);
                }
            }
        }
    }

    private interface IMeshBuilderPoint
    {
        string Key { get; }
        Vector3 Position { get; }
    }

    private class MeshBuilderCorePoint : IMeshBuilderPoint
    {
        public VoxelCell Core { get; }
        public string Key { get; }
        public Vector3 Position { get; }

        public MeshBuilderCorePoint(VoxelCell core)
        {
            Core = core;
            Key = core.ToString();
            Position = new Vector3(core.GroundPoint.Position.x, core.Height, core.GroundPoint.Position.y);
        }
        public override string ToString()
        {
            return Key;
        }
    }

    private class MeshBuilderSubPoint : IMeshBuilderPoint
    {
        public VoxelCell CellA { get; }
        public VoxelCell CellB { get; }
        public string Key { get; }

        public Vector3 Position { get; }
        public MeshBuilderSubPoint(VoxelCell cellA, VoxelCell cellB)
            :this(cellA, cellB, (cellA.CellPosition + cellB.CellPosition) / 2)
        { }
        public MeshBuilderSubPoint(VoxelCell cellA, VoxelCell cellB, Vector3 position)
        {
            bool cellAFirst = GetIsCellAFirst(cellA, cellB);
            CellA = cellAFirst ? cellA : cellB;
            CellB = cellAFirst ? cellB : cellA;
            Key = GetKey();
            Position = position;
        }

        private bool GetIsCellAFirst(VoxelCell cellA, VoxelCell cellB)
        {
            if(cellA.GroundPoint.Index == cellB.GroundPoint.Index)
            {
                if(cellA.Height == cellB.Height)
                {
                    throw new Exception("Cannot make MeshBuilderPoint from two cells at the same voxel location.");
                }
                return cellA.Height < cellB.Height;
            }
            return cellA.GroundPoint.Index < cellB.GroundPoint.Index;
        }

        private string GetKey()
        {
            return CellA.ToString() + " - " + CellB.ToString();
        }
        public override string ToString()
        {
            return Key;
        }
    }
}

public interface IHitTarget
{
   VoxelCell TargetCell { get; }
    VoxelCell SourceCell { get; }
}