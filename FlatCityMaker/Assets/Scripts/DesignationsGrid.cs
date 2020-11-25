using System.Collections.Generic;
using TileDefinition;


public class DesignationsGrid
{
    private readonly int height;
    private readonly bool[,] fillGrid;

    private readonly HashSet<TileConnectionType> filledConnectionTypes;

    public DesignationsGrid(int width, 
        int height, 
        IEnumerable<TileConnectionType> setTypes)
    {
        width++;
        height++;
        this.height = height;
        this.filledConnectionTypes = new HashSet<TileConnectionType>(setTypes);
        fillGrid = new bool[width, height];
    }

    public void ToggleGridpoint(int x, int y)
    {
        fillGrid[x, y] = !fillGrid[x, y];
    }

    public bool IsOptionAllowed(int x, int y, Tile option)
    {
        return Check(x, y, option.DownRight)
            && Check(x + 1, y, option.DownLeft)
            && Check(x, y + 1, option.UpRight)
            && Check(x + 1, y + 1, option.UpLeft);
    }

    private bool Check(int x, int y, TileConnectionType connectionType)
    {
        return fillGrid[x, y] == filledConnectionTypes.Contains(connectionType);
    }
}

