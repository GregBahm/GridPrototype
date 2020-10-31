using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TileDefinition;

public class GridCell
{
    private readonly MainGrid main;

    public int X { get; }
    public int Y { get; }

    public IReadOnlyList<Tile> OptionsFromDesignation { get; private set; }
    public Tile FilledWith { get; set; }
    
    private IReadOnlyCollection<Neighbor> neighbors;

    public GridCell(MainGrid main, int x, int y)
    {
        this.main = main;
        X = x;
        Y = y;
    }

    public void EstablishNeighbors(GridCell[,] cells)
    {
        Neighbor[] neighbors = new Neighbor[]
        {
            new Neighbor(this, cells, 0, -1, 
                item => item.BottomSideRight, item => item.TopSideRight,
                item => item.BottomSideLeft, item => item.TopSideLeft),  // down neighbor
            new Neighbor(this, cells, 0, 1,
                item => item.TopSideRight, item => item.BottomSideRight,
                item => item.TopSideLeft, item => item.BottomSideLeft),   // up neighbor
            new Neighbor(this, cells, 1, 0,
                item => item.RightSideUpper, item => item.LeftSideUpper,
                item => item.RightSideLower, item => item.LeftSideLower),  // right neighbor
            new Neighbor(this, cells, -1, 0,
                item => item.LeftSideUpper, item => item.RightSideUpper,
                item => item.LeftSideLower, item => item.RightSideLower)    // left neighbor
        };
        this.neighbors = neighbors.Where(item => item.Cell != null).ToList();
    }

    internal void UpdateContents()
    {
        Tile[] baseOptions = OptionsFromDesignation.Where(OptionIsValid).ToArray();
        if(!baseOptions.Any())
        {
            FilledWith = OptionsFromDesignation[0];
            Debug.WriteLine("Skipped. Hope somebody comes back to me later!");
            return;
            //throw new Exception("Got no options!");
        }
        Tile oldFill = FilledWith;
        FilledWith = baseOptions[0];
        if(FilledWith != oldFill)
        {
            UpdateNeighbors();
        }
    }

    private void UpdateNeighbors()
    {
        foreach (Neighbor neighbor in neighbors)
        {
            neighbor.Cell.UpdateContents();
        }
    }

    public void UpdateDesignationOptions()
    {
        // TODO: It should be possible to precompute these into a hash
        OptionsFromDesignation = main.AllOptions.Where(DesignationsAllowOption).ToArray();
        if(!OptionsFromDesignation.Any())
        {
            throw new Exception("Zero options from designation table");
        }
    }

    private bool DesignationsAllowOption(Tile option)
    {
        return main.Designations.IsOptionAllowed(X, Y, option);
    }

    private bool OptionIsValid(Tile blueprint)
    {
        return neighbors.All(item => item.CanConnectTo(blueprint));
    }

    private class Neighbor
    {
        public GridCell Cell { get; }

        private readonly Func<Tile, TileConnectionPoint> selfCornerASelector;
        private readonly Func<Tile, TileConnectionPoint> neighborCornerASelector;

        private readonly Func<Tile, TileConnectionPoint> selfCornerBSelector;
        private readonly Func<Tile, TileConnectionPoint> neighborCornerBSelector;

        public Neighbor(GridCell source,
            GridCell[,] cells, 
            int xOffset, 
            int yOffset,
            Func<Tile, TileConnectionPoint> selfCornerASelector,
            Func<Tile, TileConnectionPoint> neighborCornerASelector,
            Func<Tile, TileConnectionPoint> selfCornerBSelector,
            Func<Tile, TileConnectionPoint> neighborCornerBSelector)
        {
            int xIndex = source.X - xOffset;
            int yIndex = source.Y + yOffset;

            this.selfCornerASelector = selfCornerASelector;
            this.neighborCornerASelector = neighborCornerASelector;
            this.selfCornerBSelector = selfCornerBSelector;
            this.neighborCornerBSelector = neighborCornerBSelector;

            int width = cells.GetLength(0);
            int height = cells.GetLength(1);

            bool spaceLeft = xIndex >= 0;
            bool spaceRight = xIndex < width;
            bool spaceDown = yIndex >= 0;
            bool spaceUp = yIndex < height;
            if(spaceLeft && spaceRight && spaceDown && spaceUp)
            {
                Cell = cells[xIndex, yIndex];
            }
        }

        protected bool CanConnectTo(Tile tile, Func<Tile, TileConnectionPoint> sectorForSelf, Func<Tile, TileConnectionPoint> selectorForNeighbor)
        {
            TileConnectionPoint neighborPoint = selectorForNeighbor(Cell.FilledWith);
            if(neighborPoint.ImposesConnection)
            {
                TileConnectionPoint selfPoint = sectorForSelf(tile);
                return selfPoint.Type == neighborPoint.Type;
            }
            return true;
        }

        public virtual bool CanConnectTo(Tile tile)
        {
            return CanConnectTo(tile, selfCornerASelector, neighborCornerASelector)
                && CanConnectTo(tile, selfCornerBSelector, neighborCornerBSelector);
        }
    }

    public override string ToString()
    {
        string ret = "(" + X + "," + Y + ")";
        if(FilledWith != null)
        {
            return ret + "filled with " + FilledWith.name;
        }
        ret += " Options: ";
        foreach (var option in OptionsFromDesignation)
        {
            ret += option.Sprite.name + " ";
        }
        return ret;
    }
}

