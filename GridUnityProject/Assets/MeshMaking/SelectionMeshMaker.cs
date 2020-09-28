using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MeshBuilding;
using System;

public class SelectionMeshMaker
{
    private readonly MeshBuilderAnchorPoint basePoint;

    public Vector3[] Vertices { get; }
    public Vector3[] Normals { get; }
    public int[] Triangles { get; }

    public SelectionMeshMaker(MeshBuilderAnchorPoint basePoint)
    {
        this.basePoint = basePoint;
        Vector3[] orderedVerts = GetSurroundingAnchorVerts();

        IEnumerable<ShapeSide> sides = GetShapeSides(orderedVerts).ToArray();

        Vertices = sides.SelectMany(item => item.Vertices).ToArray();
        Normals = sides.SelectMany(item => item.Normals).ToArray();
        Triangles = sides.SelectMany(item => item.Triangles).ToArray();
    }

    private IEnumerable<ShapeSide> GetShapeSides(Vector3[] orderedVerts)
    {
        for (int i = 0; i < orderedVerts.Length; i++)
        {
            Vector3 pointA = orderedVerts[i];
            Vector3 pointB = orderedVerts[(i + 1) % orderedVerts.Length];
            yield return new ShapeSide(pointA, pointB, i);
        }
    }

    public void SetMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.vertices = Vertices;
        mesh.normals = Normals;
        mesh.triangles = Triangles;
    }

    private Vector3[] GetSurroundingAnchorVerts()
    {
        List<Vector3> baseSurround = basePoint.Connections.Select(item => item.VertPos - basePoint.VertPos).ToList();
        return baseSurround.OrderByDescending(item => Vector3.SignedAngle(item, Vector3.forward, Vector3.up)).ToArray();
    }

    private class ShapeSide
    {
        public Vector3[] Vertices { get; }
        public Vector3[] Normals { get; }
        public int[] Triangles { get; }


        public ShapeSide(Vector3 basePointA, Vector3 basePointB, int sideIndex)
        {
            int indexOffset = sideIndex * 7;
            Vector3 normal = Vector3.Cross(Vector3.up, basePointA - basePointB).normalized;

            Vertices = new Vector3[]
            {
                basePointA,
                basePointA + Vector3.up,
                basePointB,
                basePointB + Vector3.up,

                Vector3.up,
                basePointA + Vector3.up,
                basePointB + Vector3.up,
            };

            Normals = new Vector3[]
            {
                normal,
                normal,
                normal,
                normal,

                Vector3.up,
                Vector3.up,
                Vector3.up,
            };

            int baseIndex = indexOffset;
            int baseUp = indexOffset + 1;
            int next = indexOffset + 2;
            int nextUp = indexOffset + 3;

            int topA = indexOffset + 4;
            int topB = indexOffset + 5;
            int topC = indexOffset + 6;

            Triangles = new int[]
            {
                baseIndex,
                next,
                baseUp,

                nextUp,
                baseUp,
                next,

                topA,
                topB,
                topC
            };
        }
    }
}