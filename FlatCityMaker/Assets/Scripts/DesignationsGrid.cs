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

    public bool GetPointState(int x, int y)
    {
        return fillGrid[x, y];
    }

    public void ToggleGridpoint(int x, int y)
    {
        if (y >= height - 2) // No toggling the sky
        {
            return;
        }
        fillGrid[x, y] = !fillGrid[x, y];
    }

    public bool IsOptionAllowed(int x, int y, Tile option)
    {
        return Check(x, y, option.BottomSideRight.Type)
            && Check(x, y, option.RightSideLower.Type)

            && Check(x + 1, y, option.BottomSideLeft.Type)
            && Check(x + 1, y, option.LeftSideLower.Type)

            && Check(x, y + 1, option.TopSideRight.Type)
            && Check(x, y + 1, option.RightSideUpper.Type)

            && Check(x + 1, y + 1, option.TopSideLeft.Type)
            && Check(x + 1, y + 1, option.LeftSideUpper.Type);
    }

    private bool Check(int x, int y, TileConnectionType connectionType)
    {
        return fillGrid[x, y] == filledConnectionTypes.Contains(connectionType);
    }
}

