using System.Collections.Generic;
using System.Linq;
using TileDefinition;

public class MainGrid
{
    public int Width { get; }
    public int Height { get; }

    public DesignationsGrid Designations { get; }

    public IEnumerable<Tile> AllOptions { get; }

    public GridCell[,] Cells { get; private set; }

    public MainGrid(int width, int height, 
        IEnumerable<Tile> allOptions,
        DesignationsGrid designations)
    {
        Designations = designations;
        AllOptions = allOptions;
        this.Width = width;
        this.Height = height;
        Cells = CreateCells();
        foreach (GridCell cell in Cells)
        {
            cell.ResetDesignationOptions();
        }
    }

    private GridCell[,] CreateCells()
    {
        GridCell[,] ret = new GridCell[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                ret[x, y] = new GridCell(this, x, y);
            }
        }
        return ret;
    }
}

