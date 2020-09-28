using System.Collections;
using UnityEngine;
using GridMaking;
using System;
using System.Linq;

namespace MeshBuilding
{
    [RequireComponent(typeof(GridMaker))]
    class MeshBuilder : MonoBehaviour
    {
        private GridMaker gridMaker;
        private MeshBuilderGrid meshBuildeGrid;
        private MeshCollider meshCollider;
        private Mesh mesh;

        private void Start()
        {
            gridMaker = GetComponent<GridMaker>();
            meshBuildeGrid = new MeshBuilderGrid(gridMaker);
            mesh = GetMesh();
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        private void Update()
        {
            if (gridMaker.DoEase)
            {
                mesh.vertices = meshBuildeGrid.Vertices.ToArray();
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh; // Hack to force update
            }
        }

        private Mesh GetMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = meshBuildeGrid.Vertices.ToArray();
            mesh.triangles = meshBuildeGrid.Triangles;
            return mesh;
        }
    }
}