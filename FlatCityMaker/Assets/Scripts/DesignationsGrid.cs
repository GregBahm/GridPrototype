using TileDefinition;

public class DesignationsGrid
{
    private readonly int height;
    private readonly TileConnectionType filled;
    private readonly TileConnectionType sky;
    public TileConnectionType[,] Grid;

    public DesignationsGrid(int width, int height, TileConnectionType filled, TileConnectionType sky)
    {
        this.height = height;
        this.filled = filled;
        this.sky = sky;
        Grid = new TileConnectionType[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Grid[x, y] = sky;
            }
        }
    }

    public void ToggleGridpoint(int x, int y)
    {
        if(y >= height - 2) // Can't toggle the sky
        {
            return;
        }
        Grid[x, y] = Grid[x, y] == sky ? filled : sky;
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
        return connectionType == Grid[x, y];
    }
}

