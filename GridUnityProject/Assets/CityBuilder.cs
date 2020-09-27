using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridMaking;
using System;

[RequireComponent(typeof(GridMaker))]
class CityBuilder : MonoBehaviour
{
    private MeshCollider meshCollider;
    private Mesh mesh;

    private void Start()
    {
        mesh = GetMesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    private Mesh GetMesh()
    {
        Mesh mesh = new Mesh();
        return mesh;
    }
}
