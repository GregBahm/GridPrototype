using System;
using System.Linq;

public class GridCell
{
    private readonly MainGrid main;

    public int X { get; }
    public int Y { get; }

    public TileFill FilledWith { get; set; }
    
    public GridCell(MainGrid main, int x, int y)
    {
        this.main = main;
        X = x;
        Y = y;
    }

    public void UpdateFill()
    {
        NewTile tile = main.AllOptions.FirstOrDefault(DesignationsAllowOption);
        if(tile == null)
        {
            throw new Exception("Zero options from designation table");
        }
        FilledWith = GetTileFill(tile);
    }

    private TileFill GetTileFill(NewTile tile)
    {
        return new TileFill(tile.Default, tile.HorizontallyFlipped);
    }

    private bool DesignationsAllowOption(NewTile option)
    {
        return main.Designations.IsOptionAllowed(X, Y, option);
    }

    public override string ToString()
    {
        string ret = "(" + X + "," + Y + ")";
        if(FilledWith != null)
        {
            return ret + "filled with " + FilledWith.Sprite.name;
        }
        return ret;
    }
}

