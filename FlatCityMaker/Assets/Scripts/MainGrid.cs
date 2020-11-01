using System.Collections.Generic;

public class MainGrid
{
    private readonly int width;
    private readonly int height;

    public DesignationsGrid Designations { get; }

    public IEnumerable<NewTile> AllOptions { get; }

    public GridCell[,] Cells { get; private set; }

    public MainGrid(int width, int height, 
        IEnumerable<NewTile> allOptions,
        DesignationsGrid designations)
    {
        Designations = designations;
        AllOptions = allOptions;
        this.width = width;
        this.height = height;
        Cells = CreateCells();
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
        return ret;
    }
}

