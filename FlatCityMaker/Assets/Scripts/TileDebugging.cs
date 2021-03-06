﻿using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MainScript))]
public class TileDebugging : MonoBehaviour
{
    private void Start()
    {
        MainScript main = GetComponent<MainScript>();
        foreach (TileVisualBehavior visualBehavior in main.VisualTiles)
        {
            CreateOptionsDisplay(visualBehavior, main.MainGrid);
        }
    }

    private void CreateOptionsDisplay(TileVisualBehavior visualBehavior, MainGrid mainGrid)
    {
        TileDebugger display = visualBehavior.gameObject.AddComponent<TileDebugger>();
        display.Grid = mainGrid;
        display.Model = visualBehavior.Model;
    }
}

public class TileDebugger : MonoBehaviour
{
    public MainGrid Grid;
    public GridCell Model { get; set; }

    public string[] DesignationOptions;

    private void Update()
    {
        DesignationOptions = Model.OptionsFromDesignation.Select(item => item.ToString()).ToArray();
    }
}