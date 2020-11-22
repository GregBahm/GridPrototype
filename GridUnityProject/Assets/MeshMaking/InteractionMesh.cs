using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
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
            Mesh.uv = vertTable.GetUvs();
            Mesh.triangles = triangles.ToArray();
            Mesh.RecalculateBounds();
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

    public interface IHitTarget
    {
        VoxelCell TargetCell { get; }
        VoxelCell SourceCell { get; }
    }
}