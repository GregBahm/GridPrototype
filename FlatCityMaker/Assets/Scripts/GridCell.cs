using System;
using System.Linq;

public class GridCell
{
    private readonly MainGrid main;

    public int X { get; }
    public int Y { get; }

    public NewTile FilledWith { get; set; }
    
    public GridCell(MainGrid main, int x, int y)
    {
        this.main = main;
        X = x;
        Y = y;
    }

    public void UpdateFill()
    {
        NewTile tile = main.AllOptions.FirstOrDefault(OptionAllowed);
        if(tile == null)
        {
            tile = main.AllOptions.FirstOrDefault(FallbackAllowed);
            if(tile == null)
            {
                throw new Exception("Zero options from designation table");
            }
        }
        FilledWith = tile;
    }

    private bool FallbackAllowed(NewTile option)
    {
        return main.Designations.IsOptionAllowedAsFallback(X, Y, option);
    }

    private bool OptionAllowed(NewTile option)
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

