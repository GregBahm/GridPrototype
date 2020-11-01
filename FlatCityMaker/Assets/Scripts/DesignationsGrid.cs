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
        return Check(x, y, option.BottomRightFilled)
            && Check(x + 1, y, option.BottomLeftFilled)
            && Check(x, y + 1, option.TopRightFilled)
            && Check(x + 1, y + 1, option.TopLeftFilled);
    }

    private bool Check(int x, int y, bool tileRequirement)
    {
        bool isFilled = fillGrid[x, y] != TileDesignationType.Empty;
        return isFilled == tileRequirement;
    }
}
