using MeshBuilding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Voxels;

namespace Interaction
{
    public class MeshInteraction : MonoBehaviour
    {
        [SerializeField]
        private MeshBuilder meshBuilder;

        [SerializeField]
        private Transform SelectionCursor;

        private MeshBuilderAnchorPoint[] polyTable;
        private Mesh selectionMesh;
        private MeshFilter selectionMeshFilter;
        private MeshBuilderAnchorPoint selectedAnchor;

        private VoxelSpace voxelSpace;

        private void Start()
        {
            selectionMesh = new Mesh();
            selectionMeshFilter = SelectionCursor.GetComponent<MeshFilter>();
            selectionMeshFilter.sharedMesh = selectionMesh;
            polyTable = GetPolyTable();
            voxelSpace = new VoxelSpace(meshBuilder.Points, 40);
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
        public void HideSelectionMesh()
        {
            SelectionCursor.gameObject.SetActive(false);
        }

        public void PlaceMesh()
        {
            GameObject newMesh = Instantiate(SelectionCursor.gameObject);
            SelectionMeshMaker selectionMeshMaker = new SelectionMeshMaker(selectedAnchor);
            Mesh meshClone = new Mesh();
            selectionMeshMaker.SetMesh(meshClone);
            newMesh.GetComponent<MeshFilter>().mesh = meshClone;
        }

        public void ShowSelectionMesh()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (selectedAnchor != polyTable[hit.triangleIndex])
                {
                    SelectionCursor.gameObject.SetActive(true);
                    selectedAnchor = polyTable[hit.triangleIndex];
                    Shader.SetGlobalVector("_DistToCursor", selectedAnchor.VertPos);
                    SelectionCursor.position = selectedAnchor.VertPos;
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
}