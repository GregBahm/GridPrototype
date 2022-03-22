using GameGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelVisuals;

namespace MeshMaking
{
    public abstract class InteractionMesh
    {
        private IReadOnlyList<MeshHitTarget> hitTable;

        public Mesh Mesh { get; }

        public InteractionMesh()
        {
            Mesh = new Mesh();
        }

        public MeshHitTarget GetHitTarget(int hitTringleIndex)
        {
            return hitTable[hitTringleIndex];
        }

        protected  void UpdateMesh(IEnumerable<IMeshContributor> meshContributors)
        {
            InteractionVertTable vertTable = new InteractionVertTable(meshContributors);

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
            foreach (IGrouping<DesignationCell, MeshBuilderTriangle> item in meshBuidlerTriangles.GroupBy(item => item.SourceCell))
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
    }
}