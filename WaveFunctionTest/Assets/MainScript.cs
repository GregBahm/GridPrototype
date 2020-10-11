using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using UnityEngine;

public enum ConnectionType
{
    Black,
    White,
    Border,
    Tree,
    Circle,
    WhiteTree,
    LeftTree,
    RightTree
}

[Serializable]
public class ItemBlueprint
{
    public Texture2D Texture;
    public ConnectionType Up;
    public ConnectionType Down;
    public ConnectionType Left;
    public ConnectionType Right;

    public override string ToString()
    {
        return Texture.name;
    }
}

public class MainScript : MonoBehaviour
{
    public ItemBlueprint[] Blueprints;

    public int OutputWidth;
    public int OutputHeight;

    public Material BaseMaterial;

    private Grid theGrid;

    private Material[,] displayGrid;

    private void Start()
    {
        theGrid = new Grid(OutputWidth, OutputHeight, Blueprints);
        displayGrid = CreateDisplayGrid(BaseMaterial);

        theGrid.Cells[0, 0].FilledWith = Blueprints[0];
    }

    private void Update()
    {
        if(theGrid.EmptyCells.Any())
        {
            while(theGrid.DirtyCells.Any())
            {
                theGrid.DirtyCells.First().UpdateOptions();
            }

            theGrid.FillLowestEntropy();
            UpdateDisplay();
        }
    }
    private Material[,] CreateDisplayGrid(Material sourceMat)
    {
        Material[,] ret = new Material[OutputWidth, OutputHeight];
        for (int x = 0; x < OutputWidth; x++)
        {
            for (int y = 0; y < OutputHeight; y++)
            {
                GameObject newObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                newObj.name = x + " " + y;
                newObj.transform.position = new Vector3(x, y, 0);
                Material mat = new Material(sourceMat);
                newObj.GetComponent<MeshRenderer>().material = mat;
                ret[x, y] = mat;
            }
        }
        return ret;
    }
    internal void UpdateDisplay()
    {
        for (int x = 0; x < OutputWidth; x++)
        {
            for (int y = 0; y < OutputHeight; y++)
            {
                GridCell cell = theGrid.Cells[x, y];
                if (cell.FilledWith != null)
                {
                    displayGrid[x, y].SetTexture("_MainTex", cell.FilledWith.Texture);
                    displayGrid[x, y].SetColor("_Color", Color.white);
                }
                else
                {
                    displayGrid[x, y].SetColor("_Color", Color.cyan);
                }
            }
        }
    }
}
public interface IGridCell
{
    bool IsDirty { get; set; }
    bool DoesLeftConnectTo(ConnectionType type);
    bool DoesRightConnectTo(ConnectionType type);
    bool DoesUpConnectTo(ConnectionType type);
    bool DoesDownConnectTo(ConnectionType type);
}

public class PsuedoCell : IGridCell
{
    public bool IsDirty
    {
        get { return false; }
        set { }
    }

    public static IGridCell Instance { get; } = new PsuedoCell();

    public bool DoesDownConnectTo(ConnectionType type)
    {
        return true;
    }

    public bool DoesLeftConnectTo(ConnectionType type)
    {
        return true;
    }

    public bool DoesRightConnectTo(ConnectionType type)
    {
        return true;
    }

    public bool DoesUpConnectTo(ConnectionType type)
    {
        return true;
    }
}

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
        LeftNeighbor = X > 0 ? cells[X - 1, Y] : PsuedoCell.Instance;
        RightNeighbor = X < gridWidth -1 ? cells[X + 1, Y] : PsuedoCell.Instance;
        DownNeighbor = Y > 0 ? cells[X, Y - 1] : PsuedoCell.Instance;
        UpNeighbor = Y < gridHeight -1 ? cells[X, Y + 1] : PsuedoCell.Instance;
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
            bool isValid = GetIsOptionValid(option);
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
            throw new Exception("I don't have any valid options!");
        }
        Options = validOptions;
        IsDirty = false;
    }

    private void DirtyNeighbors()
    {
        LeftNeighbor.IsDirty = true;
        RightNeighbor.IsDirty = true;
        UpNeighbor.IsDirty = true;
        DownNeighbor.IsDirty = true;
    }

    private bool GetIsOptionValid(ItemBlueprint item)
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

public class Grid
{
    public HashSet<GridCell> DirtyCells { get; }

    public int Width { get; }
    public int Height { get; }

    public HashSet<GridCell> EmptyCells { get; } = new HashSet<GridCell>();
    public GridCell[,] Cells { get; }
    public Grid(int width, int height, IEnumerable<ItemBlueprint> blueprints)
    {
        Width = width;
        Height = height;
        Cells = GetCells(blueprints);
        EmptyCells = GetAllCells();
        DirtyCells = GetAllCells();
    }

    private HashSet<GridCell> GetAllCells()
    {
        HashSet<GridCell> ret = new HashSet<GridCell>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                ret.Add(Cells[x, y]);
            }
        }
        return ret;
    }

    public void FillLowestEntropy()
    {
        GridCell cellWithLeastOptions = EmptyCells.OrderBy(item => item.Options.Count).First();
        cellWithLeastOptions.FillSelfWithRandomOption();
    }

    public void FillRandomly()
    {
        int randomIndex = UnityEngine.Random.Range(0, EmptyCells.Count);
        GridCell cell = EmptyCells.ElementAt(randomIndex);
        cell.FillSelfWithRandomOption();
    }

    private GridCell[,] GetCells(IEnumerable<ItemBlueprint> options)
    {
        GridCell[,] ret = new GridCell[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                ret[x, y] = new GridCell(this, x, y, options);
            }
        }
        foreach (GridCell item in ret)
        {
            item.SetNeighbors(Width, Height, ret);
        }
        return ret;
    }
}