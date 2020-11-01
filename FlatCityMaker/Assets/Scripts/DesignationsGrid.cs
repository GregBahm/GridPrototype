public class DesignationsGrid
{
    private readonly int height;
    private readonly TileDesignationType[,] fillGrid;

    public DesignationsGrid(int width, int height)
    {
        width++;
        height++;
        this.height = height;
        fillGrid = new TileDesignationType[width, height];
    }

    public TileDesignationType GetPointState(int x, int y)
    {
        return fillGrid[x, y];
    }

    public void ToggleGridpoint(int x, int y, TileDesignationType type)
    {
        if (y >= height - 2) // No toggling the sky
        {
            return;
        }
        fillGrid[x, y] = fillGrid[x, y] == type ? TileDesignationType.Empty : type;
    }

    public bool IsOptionAllowed(int x, int y, NewTile option)
    {
        return Check(x, y, option.BottomRight)
            && Check(x + 1, y, option.BottomLeft)
            && Check(x, y + 1, option.TopRight)
            && Check(x + 1, y + 1, option.TopLeft);
    }

    public bool IsOptionAllowedAsFallback(int x, int y, NewTile option)
    {
        return FallbackCheck(x, y, option.BottomRight)
            && FallbackCheck(x + 1, y, option.BottomLeft)
            && FallbackCheck(x, y + 1, option.TopRight)
            && FallbackCheck(x + 1, y + 1, option.TopLeft);
    }
    private bool FallbackCheck(int x, int y, TileDesignationType designationType)
    {
        bool isEmpty = fillGrid[x, y] == TileDesignationType.Empty;
        if (isEmpty)
        {
            return designationType == TileDesignationType.Empty;
        }
        return designationType == TileDesignationType.DefaultFill 
            || designationType == fillGrid[x,y];
    }

    private bool Check(int x, int y, TileDesignationType designationType)
    {
        return fillGrid[x, y] == designationType;
    }
}
