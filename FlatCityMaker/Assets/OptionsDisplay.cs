using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MainScript))]
public class OptionsDisplay : MonoBehaviour
{
    private void Start()
    {
        MainScript main = GetComponent<MainScript>();
        foreach (TileVisualBehavior visualBehavior in main.VisualTiles)
        {
            CreateOptionsDisplay(visualBehavior);
        }
    }

    private void CreateOptionsDisplay(TileVisualBehavior visualBehavior)
    {
        TileOptionsDisplay display = visualBehavior.gameObject.AddComponent<TileOptionsDisplay>();
        display.Model = visualBehavior.Model;
    }
}

public class TileOptionsDisplay : MonoBehaviour
{
    public GridCell Model { get; set; }

    public string[] Options;

    private void Update()
    {
        Options = Model.OptionsFromDesignation.Select(item => item.ToString()).ToArray();
    }
}
