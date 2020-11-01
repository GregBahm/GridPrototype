﻿using System;
using System.Collections.Generic;
using System.Linq;
using TileDefinition;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public int Width;
    public int Height;
    public GameObject DisplayTilePrefab;
    public RectTransform DisplayTilesTransform;

    public Tile SkyTile;

    [SerializeField]
    private Tile[] options;

    public TileConnectionType[] FilledConnectionTypes;

    public MainGrid MainGrid { get; private set; }
    public IEnumerable<TileInteractionBehavior> InteractionTiles { get; private set; }
    public IEnumerable<TileVisualBehavior> VisualTiles { get; private set; }

    void Start()
    {
        IEnumerable<Tile> allOptions = GetSymmetricalOptions();
        DesignationsGrid designations = new DesignationsGrid(Width, Height, FilledConnectionTypes);
        MainGrid = new MainGrid(Width, Height, allOptions, designations);
        
        VisualTiles = CreateDisplayTiles();
        InteractionTiles = CreateInteractionTiles();
        FillAllWithSky();
    }

    private IEnumerable<TileInteractionBehavior> CreateInteractionTiles()
    {
        List<TileInteractionBehavior> ret = new List<TileInteractionBehavior>();
        GameObject tiles = new GameObject("InteractionTiles");
        for (int x = 0; x < Width + 1; x++)
        {
            for (int y = 0; y < Height + 1; y++)
            {
                TileInteractionBehavior behavior = CreateInteractionTile(x, y);
                behavior.gameObject.transform.parent = tiles.transform;
                ret.Add(behavior);
            }
        }
        tiles.transform.position = new Vector3(-(float)Width / 2, -(float)Height / 2);
        return ret;
    }

    private TileInteractionBehavior CreateInteractionTile(int x, int y)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = x + " " + y;
        obj.transform.position = new Vector3(x, y, 0);
        Destroy(obj.GetComponent<MeshRenderer>());
        TileInteractionBehavior behavior = obj.AddComponent<TileInteractionBehavior>();
        behavior.X = x;
        behavior.Y = y;
        behavior.ConnectedCells = MainGrid.GetCellsConnectedToDesignationPoint(x, y).ToArray();
        return behavior;
    }

    private void FillAllWithSky()
    {
        foreach (var item in MainGrid.Cells)
        {
            item.FilledWith = SkyTile;
            item.UpdateDesignationOptions();
        }
    }

    private IEnumerable<Tile> GetSymmetricalOptions()
    {
        List<Tile> ret = new List<Tile>();
        foreach (Tile option in options)
        {
            ret.Add(option);
            if (option.GetIsAsymmetrical())
            {
                ret.Add(option.GetHorizontallyFlipped());
            }
        }
        return ret;
    }

    private void Update()
    {
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(mouseRay, out hitInfo))
            {
                TileInteractionBehavior cell = hitInfo.collider.gameObject.GetComponent<TileInteractionBehavior>();
                MainGrid.Designations.ToggleGridpoint(cell.X, cell.Y);
                MainGrid.StartNewSolve(cell.ConnectedCells);
            }
        }
    }

    private IEnumerable<TileVisualBehavior> CreateDisplayTiles()
    {
        List<TileVisualBehavior> ret = new List<TileVisualBehavior>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                TileVisualBehavior behavior = CreateDisplayTile(x, y);
                behavior.transform.SetParent(DisplayTilesTransform, false);
                ret.Add(behavior);
            }
        }
        DisplayTilesTransform.position = new Vector3(-(float)Width / 2, -(float)Height / 2);
        return ret;
    }

    private TileVisualBehavior CreateDisplayTile(int x, int y)
    {
        GameObject obj = Instantiate(DisplayTilePrefab);
        obj.name = x + " " + y;
        obj.transform.position = new Vector3(x + .5f, y + .5f, 0);
        TileVisualBehavior behavior = obj.AddComponent<TileVisualBehavior>();
        behavior.Model = MainGrid.Cells[x, y];
        return behavior;
    }
}

