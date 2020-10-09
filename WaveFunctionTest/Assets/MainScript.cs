using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ConnectionType
{
    Black,
    White,
    Border,
    Tree,
    Circle
}

[Serializable]
public class ItemBlueprint
{
    public Texture2D Texture;
    public ConnectionType Up;
    public ConnectionType Down;
    public ConnectionType Left;
    public ConnectionType Right;
}

public class MainScript : MonoBehaviour
{
    public ItemBlueprint[] Blueprints;
    public int SourceWidth = 4;
    public int SourceHeight = 4;

    public int OutputWidth;
    public int OutputHeight;

    public Material BaseMaterial;

    private CurrentGrid grid;

    private void Start()
    {
        GridItem[] items = GetItems();
        grid = new CurrentGrid(OutputWidth, OutputHeight, BaseMaterial);
        grid.SetFirstCell(items);
        grid.UpdateDisplay();
    }

    private void Update()
    {
        GridPoint[] points = grid.GetAvailablePoints().ToArray();
        if (points.Any())
        {
            grid.FillNextPointTarget(points);
            grid.UpdateDisplay();
        }
    }

    private GridItem[] GetItems()
    {
        GridItem[] ret = new GridItem[Blueprints.Length];
        for (int i = 0; i < Blueprints.Length; i++)
        {
            ret[i] = new GridItem(Blueprints[i].Texture);
        }
        for (int i = 0; i < Blueprints.Length; i++)
        {
            GridItem toFill = ret[i];
            FillItem(toFill, Blueprints[i], ret);
        }
        return ret;
    }

    private void FillItem(GridItem toFill, ItemBlueprint toFillBlueprint, GridItem[] ret)
    {
        for (int i = 0; i < Blueprints.Length; i++)
        {
            GridItem otherItem = ret[i];
            ItemBlueprint bluePrint = Blueprints[i];
            FillItem(toFill, toFillBlueprint, otherItem, bluePrint);
        }
    }

    private void FillItem(GridItem itemA, ItemBlueprint blueprintA, GridItem itemB, ItemBlueprint blueprintB)
    {
        if (blueprintA.Left == blueprintB.Right)
        {
            itemA.LeftOptions.Add(itemB);
        }
        if (blueprintA.Right == blueprintB.Left)
        {
            itemA.RightOptions.Add(itemB);
        }
        if (blueprintA.Up == blueprintB.Down)
        {
            itemA.UpOptions.Add(itemB);
        }
        if (blueprintA.Down == blueprintB.Up)
        {
            itemA.DownOptions.Add(itemB);
        }
    }
}

public class CurrentGrid
{
    private readonly int width;
    private readonly int height;
    public GridItem[,] Grid { get; }
    public Material[,] DisplayGrid { get; }

    public bool[,] AvailabilityGrid { get; }

    public CurrentGrid(int width, int height, Material sourceMat)
    {
        this.width = width;
        this.height = height;
        Grid = new GridItem[width, height];
        AvailabilityGrid = new bool[width, height];
        DisplayGrid = CreateDisplayGrid(sourceMat);
    }

    private Material[,] CreateDisplayGrid(Material sourceMat)
    {
        Material[,] ret = new Material[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                newObj.transform.position = new Vector3(x, y, 0);
                Material mat = new Material(sourceMat);
                newObj.GetComponent<MeshRenderer>().material = mat;
                ret[x, y] = mat;
            }
        }
        return ret;
    }

    public void SetCell(int x, int y, GridItem item)
    {
        Grid[x, y] = item;
        AvailabilityGrid[x, y] = false;
        if (x < width - 1)
            AvailabilityGrid[x + 1, y] = Grid[x + 1, y] == null;
        if (y < height - 1)
            AvailabilityGrid[x, y + 1] = Grid[x, y + 1] == null;
        if (x > 0)
            AvailabilityGrid[x - 1, y] = Grid[x - 1, y] == null;
        if (y > 0)
            AvailabilityGrid[x, y - 1] = Grid[x, y - 1] == null;
    }

    public void FillNextPointTarget(GridPoint[] availablePoints)
    {
        int randomIndex = UnityEngine.Random.Range(0, availablePoints.Length - 1);
        GridPoint toDo = availablePoints[randomIndex];
        DoCell(toDo.X, toDo.Y);
    }

    private void DoCell(int x, int y)
    {
        CellFiller cellFiller = GetCellFiller(x, y);
        GridItem item = cellFiller.GetSolution();
        SetCell(x, y, item);
    }

    private CellFiller GetCellFiller(int x, int y)
    {
        GridItem up = null;
        GridItem left = null;
        GridItem down = null;
        GridItem right = null;
        if (y < height - 1)
            up = Grid[x, y + 1];
        if (x > 0)
            left = Grid[x - 1, y];
        if (y > 0)
            down = Grid[x, y - 1];
        if (x < width - 1)
            right = Grid[x + 1, y];
        return new CellFiller(up, left, down, right);
    }

    public IEnumerable<GridPoint> GetAvailablePoints()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (AvailabilityGrid[x, y])
                    yield return new GridPoint(x, y);
            }
        }
    }

    internal void UpdateDisplay()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (Grid[x, y] != null)
                {
                    DisplayGrid[x, y].SetTexture("_MainTex", Grid[x, y].Texture);
                }
                Color color = AvailabilityGrid[x, y] ? Color.red : Color.white;
                DisplayGrid[x, y].SetColor("_Color", color);
            }
        }
    }

    internal void SetFirstCell(GridItem[] items)
    {
        int randomIndex = UnityEngine.Random.Range(0, items.Length);
        int randomX = UnityEngine.Random.Range(0, width);
        int randomY = UnityEngine.Random.Range(0, height);
        SetCell(randomX, randomY, items[randomIndex]);
    }
}

public struct GridPoint
{
    public int X { get; }
    public int Y { get; }

    public GridPoint(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class CellFiller
{
    public GridItem Up { get; }
    public GridItem Left { get; }
    public GridItem Right { get; }
    public GridItem Down { get; }

    public CellFiller(GridItem up, GridItem left, GridItem down, GridItem right)
    {
        if (up == null && left == null && right == null && down == null)
        {
            throw new Exception("Trying to make a cell filler surrounded by nulls");
        }
        Up = up;
        Left = left;
        Down = down;
        Right = right;
    }

    public GridItem[] GetAvailableOptions()
    {
        List<HashSet<GridItem>> optionSets = new List<HashSet<GridItem>>();
        if (Up != null)
        {
            optionSets.Add(Up.DownOptions);
        }
        if (Left != null)
        {
            optionSets.Add(Left.RightOptions);
        }
        if (Right != null)
        {
            optionSets.Add(Right.LeftOptions);
        }
        if (Down != null)
        {
            optionSets.Add(Down.UpOptions);
        }

        GridItem[] ret = optionSets[0].ToArray();
        if (optionSets.Count > 1)
        {
            foreach (HashSet<GridItem> item in optionSets.Skip(1))
            {
                ret = ret.Intersect(item).ToArray();
            }
        }
        return ret;
    }

    public GridItem GetSolution()
    {
        GridItem[] availableOptions = GetAvailableOptions();
        if (availableOptions.Any())
        {
            int randomIndex = UnityEngine.Random.Range(0, availableOptions.Length - 1);
            return availableOptions[randomIndex];
        }
        return null;
    }
}


public class GridItem
{
    public Texture2D Texture { get; }
    public HashSet<GridItem> RightOptions { get; } = new HashSet<GridItem>();
    public HashSet<GridItem> LeftOptions { get; } = new HashSet<GridItem>();
    public HashSet<GridItem> UpOptions { get; } = new HashSet<GridItem>();
    public HashSet<GridItem> DownOptions { get; } = new HashSet<GridItem>();

    public GridItem(Texture2D texture)
    {
        Texture = texture;
    }
}
