﻿using GameGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
    public class InteractionMesh
    {
        private IReadOnlyList<MeshHitTarget> hitTable;

        public Mesh Mesh { get; }

        public InteractionMesh(Mesh mesh)
        {
            Mesh = mesh;
        }

        public MeshHitTarget GetHitTarget(int hitTringleIndex)
        {
            return hitTable[hitTringleIndex];
        }

        public void UpdateMesh(MainGrid grid)
        {
            IEnumerable<IMeshContributor> meshContributors = GetMeshContributors(grid).ToArray();
            VertTable vertTable = new VertTable(meshContributors);

            Mesh.Clear();
            Mesh.vertices = vertTable.GetPoints();

            List<MeshBuilderTriangle> meshBuidlerTriangles = new List<MeshBuilderTriangle>();
            List<int> triangles = new List<int>();
            foreach (IMeshContributor item in meshContributors)
            {
                foreach (MeshBuilderTriangle triangle in item.Triangles)
                {
                    triangles.Add(vertTable.GetVertIndex(triangle.PointA));
                    triangles.Add(vertTable.GetVertIndex(triangle.PointB));
                    triangles.Add(vertTable.GetVertIndex(triangle.PointC));
                    meshBuidlerTriangles.Add(triangle);
                }
            }
            Mesh.uv = vertTable.GetUvs();
            Mesh.triangles = triangles.ToArray();
            Mesh.RecalculateBounds();
            hitTable = CreateHittable(meshBuidlerTriangles);
        }

        private IReadOnlyList<MeshHitTarget> CreateHittable(List<MeshBuilderTriangle> meshBuidlerTriangles)
        {
            MeshHitTarget[] ret = new MeshHitTarget[meshBuidlerTriangles.Count];
            Dictionary<MeshBuilderTriangle, int> indexTable = new Dictionary<MeshBuilderTriangle, int>();
            for (int i = 0; i < meshBuidlerTriangles.Count; i++)
            {
                indexTable.Add(meshBuidlerTriangles[i], i);
            }
            foreach (IGrouping<VoxelCell, MeshBuilderTriangle> item in meshBuidlerTriangles.GroupBy(item => item.SourceCell))
            {
                foreach(var subItem in item.GroupBy(subItem => subItem.TargetCell))
                {
                    MeshBuilderTriangle firstValue = subItem.First();
                    MeshHitTarget hitTarget = new MeshHitTarget(firstValue.TargetCell, firstValue.SourceCell, subItem);
                    foreach (MeshBuilderTriangle bedrock in subItem)
                    {
                        int index = indexTable[bedrock];
                        ret[index] = hitTarget;
                    }
                }
            }
            return ret.ToList().AsReadOnly();
        }

        private class VertTable
        {
            private readonly List<Vector3> pointPositions = new List<Vector3>();
            private readonly List<Vector2> uvs = new List<Vector2>();
            private readonly Dictionary<string, int> indexTable = new Dictionary<string, int>();

            public VertTable(IEnumerable<IMeshContributor> meshContributors)
            {
                foreach (IMeshBuilderPoint point in meshContributors.SelectMany(item => item.Points))
                {
                    if (!indexTable.ContainsKey(point.Key))
                    {
                        indexTable.Add(point.Key, pointPositions.Count);
                        pointPositions.Add(point.Position);
                        uvs.Add(point.Uv);
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

            internal Vector2[] GetUvs()
            {
                return uvs.ToArray();
            }
        }

        private IEnumerable<IMeshContributor> GetMeshContributors(MainGrid grid)
        {
            IMeshContributor[] groundContributor = grid.Points.Where(item => !item.Voxels[0].Filled).Select(item => new HorizontalMeshContributor(item)).ToArray();
            IMeshContributor[] contributors = grid.FilledCells.Select(item => new CellMeshContributor(item)).ToArray();
            return groundContributor.Concat(contributors);
        }
    }
}
