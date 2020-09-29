using MeshBuilding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Interaction
{
    [RequireComponent(typeof(MeshBuilder))]
    public class MeshInteraction : MonoBehaviour
    {
        private MeshBuilderAnchorPoint[] polyTable;
        private MeshBuilder meshBuilder;

        private Mesh selectionMesh;
        private MeshFilter selectionMeshFilter;

        [SerializeField]
        private Transform Selection;

        private MeshBuilderAnchorPoint selectedAnchor;

        private void Start()
        {
            selectionMesh = new Mesh();
            selectionMeshFilter = Selection.GetComponent<MeshFilter>();
            selectionMeshFilter.sharedMesh = selectionMesh;
            meshBuilder = GetComponent<MeshBuilder>();
            polyTable = GetPolyTable();
        }

        private MeshBuilderAnchorPoint[] GetPolyTable()
        {
            MeshBuilderAnchorPoint[] ret = new MeshBuilderAnchorPoint[meshBuilder.TriangleCount];
            foreach (MeshBuilderPoly poly in meshBuilder.Polys)
            {
                foreach (var item in poly.TriangleInteractionBindings)
                {
                    ret[item.TriangleIndex] = item.AnchorPoint;
                }
            }
            return ret;
        }

        private void Update()
        {
            PlaceDebugCube();
            if (selectedAnchor != null && Input.GetMouseButtonDown(0))
            {
                GameObject newBox = Instantiate(Selection.gameObject);
                SelectionMeshMaker selectionMeshMaker = new SelectionMeshMaker(selectedAnchor);
                Mesh meshClone = new Mesh();
                selectionMeshMaker.SetMesh(meshClone);
                newBox.GetComponent<MeshFilter>().mesh = meshClone;
            }
        }

        private void PlaceDebugCube()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (selectedAnchor != polyTable[hit.triangleIndex])
                {
                    selectedAnchor = polyTable[hit.triangleIndex];
                    Shader.SetGlobalVector("_DistToCursor", selectedAnchor.VertPos);
                    Selection.position = selectedAnchor.VertPos;
                    UpdateCubeMesh();
                }
            }
        }

        private void UpdateCubeMesh()
        {
            SelectionMeshMaker selectionMeshMaker = new SelectionMeshMaker(selectedAnchor);
            selectionMeshMaker.SetMesh(selectionMesh);
        }
    }

    public class CameraInteraction : MonoBehaviour
    {
        private Transform orbitPoint;

        private void Start()
        {
            orbitPoint = new GameObject("Camera Orbit").transform;
        }

        public void StartOrbit()
        {
            Plane plane;
        }

        public void ContinueOrbit()
        {

        }
    }
}