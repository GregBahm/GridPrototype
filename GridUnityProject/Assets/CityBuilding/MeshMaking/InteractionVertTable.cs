using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshMaking
{
    internal class InteractionVertTable
    {
        private readonly List<Vector3> pointPositions = new List<Vector3>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly Dictionary<string, int> indexTable = new Dictionary<string, int>();

        public InteractionVertTable(IEnumerable<IMeshContributor> meshContributors)
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
}
