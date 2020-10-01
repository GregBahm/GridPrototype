using System.Collections;
using UnityEngine;
using GridMaking;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MeshBuilding
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(GridMaker))]
    class MeshBuilder : MonoBehaviour
    {
        private bool wasEasing;
        private GridMaker gridMaker;
        private MeshBuilderGrid meshBuildeGrid;
        private MeshCollider meshCollider;
        private Mesh mesh;

        public IEnumerable<MeshBuilderPoly> Polys { get { return meshBuildeGrid.Polys; } }
        public IEnumerable<MeshBuilderAnchorPoint> Points { get { return meshBuildeGrid.Points; } }
        public int TriangleCount { get; private set; }

        private void Start()
        {
            gridMaker = GetComponent<GridMaker>();
            meshBuildeGrid = new MeshBuilderGrid(gridMaker);
            mesh = GetMesh();
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            TriangleCount = mesh.triangles.Length / 3;
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private void Update()
        {
            UpdateForEasing();
        }

        private void UpdateForEasing()
        {
            if (!gridMaker.DoEase && wasEasing)
            {
                mesh.vertices = meshBuildeGrid.Vertices.ToArray();
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh; // Hack to force update
            }
            wasEasing = gridMaker.DoEase;
        }

        private Mesh GetMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = meshBuildeGrid.Vertices.ToArray();
            mesh.uv = meshBuildeGrid.Uvs;
            mesh.triangles = meshBuildeGrid.Triangles;
            return mesh;
        }
    }
}