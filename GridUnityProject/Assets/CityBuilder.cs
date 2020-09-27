using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridMaking;
using System;

[RequireComponent(typeof(GridMaker))]
class CityBuilder : MonoBehaviour
{
    private GridMaker gridMaker;
    private CityBuilderGrid cityBuildeGrid;
    private MeshCollider meshCollider;
    private Mesh mesh;

    private void Start()
    {
        gridMaker = GetComponent<GridMaker>();
        cityBuildeGrid = new CityBuilderGrid(gridMaker);
        mesh = GetMesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    private Mesh GetMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = cityBuildeGrid.Vertices;
        mesh.triangles = cityBuildeGrid.Triangles;
        return mesh;
    }
}

public class CityBuilderGrid
{
    private GridMaker gridMaker;

    public Vector3[] Vertices { get; }
    public int[] Triangles { get; }

    public CityBuilderGrid(GridMaker gridMaker)
    {
        this.gridMaker = gridMaker;
        //TODO: Build thse objects
    }
}

public class CityBuilderAnchorPoint
{
    public EasedPoint Anchor { get; }
    public Vector3 Pos { get; }
    public int Index { get; }
}

public class CityBuilderEdge
{
    public CityBuilderAnchorPoint PointA { get; }
    public CityBuilderAnchorPoint PointB { get; }

    Vector3 CenterPos { get; }
    public int CenterIndex;
}

public class CityBuilderPoly
{
    public CityBuilderAnchorPoint BasePointA { get; }
    public CityBuilderAnchorPoint BasePointB { get; }
    public CityBuilderAnchorPoint BasePointC { get; }
    public CityBuilderAnchorPoint BasePointD { get; }

    public CityBuilderEdge EdgeAB { get; }
    public CityBuilderEdge EdgeBC { get; }
    public CityBuilderEdge EdgeCD { get; }
    public CityBuilderEdge EdgeAD { get; }

    public Vector3 CenterPos;
    public int CenterIndex { get; }
}