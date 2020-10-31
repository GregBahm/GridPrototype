using System;
using System.Collections.Generic;
using System.Linq;
using TileDefinition;
using UnityEditor.PackageManager.Requests;

public class MainGrid
{
    private readonly int width;
    private readonly int height;

    public DesignationsGrid Designations { get; }

    public IEnumerable<Tile> AllOptions { get; }

    public GridCell[,] Cells { get; private set; }

    public MainGrid(int width, int height, 
        IEnumerable<Tile> allOptions,
        DesignationsGrid designations)
    {
        Designations = designations;
        AllOptions = allOptions;
        this.width = width;
        this.height = height;
        Cells = CreateCells();
    }

    public void StartNewSolve(IEnumerable<GridCell> cellsToReset)
    {
        foreach (GridCell cell in cellsToReset)
        {
            cell.UpdateDesignationOptions();
        }
        foreach (GridCell cell in cellsToReset)
        {
            cell.UpdateContents();
        }
    }

    private HashSet<GridCell> GetAllCells()
    {
        HashSet<GridCell> ret = new HashSet<GridCell>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ret.Add(Cells[x, y]);
            }
        }
        return ret;
    }

    public IEnumerable<GridCell> GetCellsConnectedToDesignationPoint(int x, int y)
    {
        x--;
        y--;
        if (x > 0 && y > 0)
            yield return Cells[x, y];
        if (x < width - 2 && y > 0)
            yield return Cells[x + 1, y];
        if (x > 0 && y < height - 1)
            yield return Cells[x, y + 1];
        if (x < width - 1 && y < height - 1)
            yield return Cells[x + 1, y + 1];
    }

    private GridCell[,] CreateCells()
    {
        GridCell[,] ret = new GridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ret[x, y] = new GridCell(this, x, y);
            }
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ret[x, y].EstablishNeighbors(ret);
            }
        }
        return ret;
    }
}

