using System;
using System.Collections.Generic;
using System.Linq;
using TileDefinition;

public class GridCell
{
    private readonly MainGrid main;

    public int X { get; }
    public int Y { get; }

    public Tile FilledWith { get; set; }

    public IReadOnlyList<Tile> OptionsFromDesignation { get; private set; }
    
    public GridCell(MainGrid main, int x, int y)
    {
        this.main = main;
        X = x;
        Y = y;
    }

    public void ResetDesignationOptions()
    {
        // TODO: It should be possible to precompute these into a hash
        OptionsFromDesignation = main.AllOptions.Where(DesignationsAllowOption).ToArray();
    }

    private bool DesignationsAllowOption(Tile option)
    {
        return main.Designations.IsOptionAllowed(X, Y, option);
    }
}