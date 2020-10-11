using System;
using System.Collections;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public ItemBlueprint[] Blueprints;

    public int OutputWidth;
    public int OutputHeight;

    public Material BaseMaterial;

    private Grid theGrid;

    private Material[,] displayGrid;

    private const int loopLimit = 10000;

    private void Start()
    {
        theGrid = new Grid(OutputWidth, OutputHeight, Blueprints);
        displayGrid = CreateDisplayGrid(BaseMaterial);
        theGrid.FillRandomly();
    }
    
    private void Update()
    {
        if(theGrid.EmptyCells.Any())
        {
            if(theGrid.DirtyCells.Any())
            {
                UpdateOptions();
            }
            else
            {
                theGrid.FillLowestEntropy();
                UpdateDisplay();
            }
        }
    }

    private void UpdateOptions()
    {
        int updateCount = 0;
        while(theGrid.DirtyCells.Any() && updateCount < loopLimit)
        {
            theGrid.DirtyCells.First().UpdateOptions();
            updateCount++;
        }
    }

    private Material[,] CreateDisplayGrid(Material sourceMat)
    {
        Material[,] ret = new Material[OutputWidth, OutputHeight];
        for (int x = 0; x < OutputWidth; x++)
        {
            for (int y = 0; y < OutputHeight; y++)
            {
                GameObject newObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                newObj.name = x + " " + y;
                newObj.transform.position = new Vector3(x, y, 0);
                Material mat = new Material(sourceMat);
                newObj.GetComponent<MeshRenderer>().material = mat;
                ret[x, y] = mat;
            }
        }
        return ret;
    }
    internal void UpdateDisplay()
    {
        for (int x = 0; x < OutputWidth; x++)
        {
            for (int y = 0; y < OutputHeight; y++)
            {
                GridCell cell = theGrid.Cells[x, y];
                if (cell.FilledWith != null)
                {
                    displayGrid[x, y].SetTexture("_MainTex", cell.FilledWith.Texture);
                    displayGrid[x, y].SetColor("_Color", Color.white);
                }
                else
                {
                    displayGrid[x, y].SetColor("_Color", Color.cyan);
                }
            }
        }
    }
}
