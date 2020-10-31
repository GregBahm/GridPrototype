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

    public HashSet<GridCell> SolvedCells { get; } = new HashSet<GridCell>();
    public HashSet<GridCell> EmptyCells { get; } = new HashSet<GridCell>();

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

    public void StartNewSolve()
    {
        if(EmptyCells.Any())
        {
            throw new Exception("Can't start new solve when a solve is already in progress");
        }
        SolvedCells.Clear();
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

    public void TryFillLowestEntropy()
    {
        IEnumerable<GridCell> emptyCells = EmptyCells.ToArray();
        foreach (GridCell cell in emptyCells)
        {
            cell.UpdateEmptyCell();
        }
        if(EmptyCells.Any())
        {
            GridCell cellWithLeastOptions = EmptyCells.OrderBy(item => item.Options.Count).First();
            cellWithLeastOptions.FillSelf();
        }
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

