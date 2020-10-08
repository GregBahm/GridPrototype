using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

public class HittestMesh
{
    private readonly List<IHitTarget> hitTable = new List<IHitTarget>();

    public Mesh Mesh { get; }

    public HittestMesh(Mesh mesh)
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
        IMeshContributor[] groundContributor = grid.Points.Where(item => !item.Voxels[0].Filled).Select(item => new GroundMeshContributor(item)).ToArray();
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

        public MeshBuilderTriangle(VoxelCell targetCell, VoxelCell sourceCell, IMeshBuilderPoint pointA, IMeshBuilderPoint pointB, IMeshBuilderPoint pointC)
        {
            TargetCell = TargetCell;
            SourceCell = sourceCell;
            PointA = pointA;
            bool reorderVerts = GetShouldReorderVerts(sourceCell, pointA.Position, pointB.Position, pointC.Position);
            PointB = reorderVerts ? pointB : pointC;
            PointC = reorderVerts ? pointC : pointB;
        }

        private bool GetShouldReorderVerts(VoxelCell sourceCell, Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 crossVector = Vector3.Cross(pointA - pointB, pointA - pointC);
            if(sourceCell == null) // it's a ground cell. Just make sure the cross product points upward
            {
                return crossVector.y > 0;
            }
            return Vector3.Dot(sourceCell.CellPosition, crossVector) > 0; // TODO: Test this
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

    private class GroundMeshContributor : IMeshContributor
    {
        public IEnumerable<IMeshBuilderPoint> Points { get; }

        public IEnumerable<MeshBuilderTriangle> Triangles { get; }

        public GroundMeshContributor(GroundPoint groundPoint)
        {
            List<IMeshBuilderPoint> points = new List<IMeshBuilderPoint>();
            List<MeshBuilderTriangle> triangles = new List<MeshBuilderTriangle>();

            VoxelCell baseCell = groundPoint.Voxels[0];
            IMeshBuilderPoint corePoint = new MeshBuilderCorePoint(baseCell);

            points.Add(corePoint);
            foreach (GroundQuad quad in groundPoint.PolyConnections)
            {
                GroundPoint diagonal = quad.GetDiagonalPoint(groundPoint);
                Vector3 quadPos = new Vector3(quad.Center.x, 0, quad.Center.y);
                IMeshBuilderPoint diagonalPoint = new MeshBuilderSubPoint(baseCell, diagonal.Voxels[0], quadPos);
                GroundPoint[] otherPoints = quad.Points.Where(item => item != groundPoint && item != diagonal).ToArray();
                IMeshBuilderPoint otherPointA = new MeshBuilderSubPoint(baseCell, otherPoints[0].Voxels[0]);
                IMeshBuilderPoint otherPointB = new MeshBuilderSubPoint(baseCell, otherPoints[1].Voxels[0]);

                MeshBuilderTriangle triangleA = new MeshBuilderTriangle(baseCell, null, corePoint, diagonalPoint, otherPointA);
                MeshBuilderTriangle triangleB = new MeshBuilderTriangle(baseCell, null, diagonalPoint, corePoint, otherPointB);

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
            Points = GetPoints();
            Triangles = GetTriangles();
        }

        private IEnumerable<MeshBuilderTriangle> GetTriangles()
        {
            return new MeshBuilderTriangle[0]; // TODO: This
        }

        private IEnumerable<IMeshBuilderPoint> GetPoints()
        {
            return new IMeshBuilderPoint[0]; // TODO: This
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