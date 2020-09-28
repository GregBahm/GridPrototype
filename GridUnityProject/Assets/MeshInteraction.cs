using MeshBuilding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    private void PlaceDebugCube()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if(selectedAnchor != polyTable[hit.triangleIndex])
            {
                selectedAnchor = polyTable[hit.triangleIndex];
                Selection.position = selectedAnchor.VertPos;
                UpdateCubeMesh();
            }
        }
    }

    private void UpdateCubeMesh()
    {
        selectedAnchor.SetSelectionMesh(selectionMesh);
    }
}
