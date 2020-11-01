using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MainScript))]
public class DesignationTableDisplay : MonoBehaviour
{
    private DesignationsGrid grid;
    
    public Material DisplayMat;

    public Color ToggledColor;

    public Color SkyColor;


    private void Start()
    {
        MainScript main = GetComponent<MainScript>();
        grid = main.MainGrid.Designations;
        MakeDesignationDisplayPoints(main.InteractionTiles);
    }
    
    private void MakeDesignationDisplayPoints(IEnumerable<TileInteractionBehavior> interactionTiles)
    {
        foreach (TileInteractionBehavior tile in interactionTiles)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere.GetComponent<MeshCollider>());
            DesignationDisplay display = sphere.AddComponent<DesignationDisplay>();
            display.Mat = new Material(DisplayMat);
            display.Source = this;
            display.Grid = grid;
            display.DesignationPoint = tile;
            sphere.transform.parent = tile.transform;
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = new Vector3(.1f, .1f, .1f);
        }
    }
}

public class DesignationDisplay : MonoBehaviour
{
    public DesignationTableDisplay Source;
    public DesignationsGrid Grid;
    public TileInteractionBehavior DesignationPoint;
    public Material Mat;

    private void Start()
    {
        GetComponent<MeshRenderer>().material = Mat;
    }

    private void Update()
    {
        bool isToggled = Grid.GetPointState(DesignationPoint.X, DesignationPoint.Y);
        Mat.SetColor("_Color", isToggled ? Source.ToggledColor : Source.SkyColor);
    }
}