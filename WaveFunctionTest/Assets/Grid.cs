using System.Collections.Generic;
using System.Linq;

public class Grid
{
    public HashSet<GridCell> DirtyCells { get; }

    public int Width { get; }
    public int Height { get; }

    public HashSet<GridCell> EmptyCells { get; } = new HashSet<GridCell>();
    public GridCell[,] Cells { get; }
    public Grid(int width, int height, IEnumerable<ItemBlueprint> blueprints)
    {
        Width = width;
        Height = height;
        Cells = GetCells(blueprints);
        EmptyCells = GetAllCells();
        DirtyCells = GetAllCells();
    }

    private HashSet<GridCell> GetAllCells()
    {
        HashSet<GridCell> ret = new HashSet<GridCell>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                ret.Add(Cells[x, y]);
            }
        }
        return ret;
    }

    public void FillLowestEntropy()
    {
        GridCell cellWithLeastOptions = EmptyCells.OrderBy(item => item.Options.Count).First();
        cellWithLeastOptions.FillSelfWithRandomOption();
    }

    public void FillRandomly()
    {
        int randomIndex = UnityEngine.Random.Range(0, EmptyCells.Count);
        GridCell cell = EmptyCells.ElementAt(randomIndex);
        cell.FillSelfWithRandomOption();
    }

    private GridCell[,] GetCells(IEnumerable<ItemBlueprint> options)
    {
        GridCell[,] ret = new GridCell[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                ret[x, y] = new GridCell(this, x, y, options);
            }
        }
        foreach (GridCell item in ret)
        {
            item.SetNeighbors(Width, Height, ret);
        }
        return ret;
    }
}