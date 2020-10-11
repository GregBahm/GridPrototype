using System;
using System.Collections.Generic;
using System.Linq;

public class GridCell : IGridCell
{
    private readonly Grid grid;

    public int X { get; }
    public int Y { get; }

    public bool IsDirty
    {
        get 
        {
            if (FilledWith != null)
                return false;
            return grid.DirtyCells.Contains(this); 
        }
        set
        {
            if(FilledWith == null)
            {
                if (value)
                {
                    grid.DirtyCells.Add(this);
                }
                else
                {
                    grid.DirtyCells.Remove(this);
                }
            }
        }
    }

    private ItemBlueprint filledWIth;
    public ItemBlueprint FilledWith
    {
        get { return filledWIth; }
        set
        {
            filledWIth = value;
            if(value != null)
            {
                Options = new ItemBlueprint[] { value };
                grid.EmptyCells.Remove(this);
                grid.DirtyCells.Remove(this);
            }
            DirtyNeighbors();
        }
    }

    public IReadOnlyList<ItemBlueprint> Options { get; private set; }

    public IGridCell LeftNeighbor { get; private set; }
    public IGridCell RightNeighbor { get; private set; }
    public IGridCell UpNeighbor { get; private set; }
    public IGridCell DownNeighbor { get; private set; }

    public GridCell(Grid grid, int x, int y, IEnumerable<ItemBlueprint> options)
    {
        this.grid = grid;
        X = x;
        Y = y;
        Options = options.ToList();
    }

    public void SetNeighbors(int gridWidth, int gridHeight, GridCell[,] cells)
    {
        LeftNeighbor = X > 0 ? cells[X - 1, Y] : OffGridCell.Instance;
        RightNeighbor = X < gridWidth -1 ? cells[X + 1, Y] : OffGridCell.Instance;
        DownNeighbor = Y > 0 ? cells[X, Y - 1] : OffGridCell.Instance;
        UpNeighbor = Y < gridHeight -1 ? cells[X, Y + 1] : OffGridCell.Instance;
    }

    public void UpdateOptions()
    {
        if(!IsDirty)
        {
            throw new Exception("I shouldn't be updating. I'm already clean.");
        }
        List<ItemBlueprint> validOptions = new List<ItemBlueprint>();
        foreach (ItemBlueprint option in Options)
        {
            bool isValid = GetIsValidByPlacement(option);
            if(isValid)
            {
                validOptions.Add(option);
            }
            else
            {
                DirtyNeighbors();
            }
        }
        if(!validOptions.Any())
        {
            HandleContradiction();
        }
        else
        {
            Options = validOptions;
            IsDirty = false;
        }
    }

    public void ResetOptions()
    {
        Options = grid.AllOptions;
        IsDirty = true;
        FilledWith = null;
    }

    private void HandleContradiction()
    {
        ResetOptions();
        LeftNeighbor.ResetOptions();
        RightNeighbor.ResetOptions();
        UpNeighbor.ResetOptions();
        DownNeighbor.ResetOptions();
    }

    private void DirtyNeighbors()
    {
        LeftNeighbor.IsDirty = true;
        RightNeighbor.IsDirty = true;
        UpNeighbor.IsDirty = true;
        DownNeighbor.IsDirty = true;
    }

    /// <summary>
    /// Checks that the neighbors of the cell allow this option
    /// </summary>
    private bool GetIsValidByPlacement(ItemBlueprint item)
    {
        return UpNeighbor.DoesDownConnectTo(item.Up)
               && DownNeighbor.DoesUpConnectTo(item.Down)
               && LeftNeighbor.DoesRightConnectTo(item.Left)
               && RightNeighbor.DoesLeftConnectTo(item.Right);
    }

    public bool DoesLeftConnectTo(ConnectionType type)
    {
        return Options.Any(item => item.Left == type);
    }

    public bool DoesRightConnectTo(ConnectionType type)
    {
        return Options.Any(item => item.Right == type);
    }

    public bool DoesUpConnectTo(ConnectionType type)
    {
        return Options.Any(item => item.Up == type);
    }

    public bool DoesDownConnectTo(ConnectionType type)
    {
        return Options.Any(item => item.Down == type);
    }

    internal void FillSelfWithRandomOption()
    {
        ItemBlueprint[] optionsArray = Options.ToArray();
        int rand = UnityEngine.Random.Range(0, optionsArray.Length);
        FilledWith = optionsArray[rand];
    }
}
