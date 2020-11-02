using System.Collections.Generic;
using System.Linq;
using TileDefinition;

public class MainGrid
{
    private readonly int width;
    private readonly int height;

    public DesignationsGrid Designations { get; }

    public IEnumerable<Tile> AllOptions { get; }

    public HashSet<GridCell> RefreshedCells { get; } = new HashSet<GridCell>();

    public HashSet<GridCell> DirtyCells { get; } = new HashSet<GridCell>();

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
        foreach (GridCell cell in Cells)
        {
            cell.ResetDesignationOptions();
        }
        DirtyCells.Clear();
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

