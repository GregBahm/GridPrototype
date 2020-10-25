using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour
{
    public int Width;
    public int Height;
    public GameObject DisplayTilePrefab;
    public Canvas MainCanvas;

    public int SkyIndex;

    [SerializeField]
    private CellVisualBlueprint[] options;

    private MainGrid mainGrid;

    private CellVisualBlueprint sky;

    void Start()
    {
        sky = options[SkyIndex];
        IEnumerable<CellVisualBlueprint> allOptions = GetSymmetricalOptions();
        mainGrid = new MainGrid(Width, Height, allOptions);
        
        CreateDisplayTiles();
        CreateInteractionTiles();
        ResetGridFills();
    }

    private void CreateInteractionTiles()
    {
        GameObject tiles = new GameObject("InteractionTiles");
        for (int x = 0; x < Width + 1; x++)
        {
            for (int y = 0; y < Height + 1; y++)
            {
                GameObject obj = CreateInteractionTile(x, y);
                obj.transform.parent = tiles.transform;
            }
        }
        tiles.transform.position = new Vector3(-(float)Width / 2, -(float)Height / 2);
    }

    private GameObject CreateInteractionTile(int x, int y)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = x + " " + y;
        obj.transform.position = new Vector3(x, y, 0);
        Destroy(obj.GetComponent<MeshRenderer>());
        TileInteractionBehavior behavior = obj.AddComponent<TileInteractionBehavior>();
        behavior.X = x;
        behavior.Y = y;
        return obj;
    }

    private void ResetGridFills()
    {
        foreach (var item in mainGrid.Cells)
        {
            item.Reset();
        }
        for (int x = 0; x < Width; x++)
        {
            mainGrid.Cells[x, Height - 1].FilledWith = sky;
        }
    }

    private IEnumerable<CellVisualBlueprint> GetSymmetricalOptions()
    {
        List<CellVisualBlueprint> ret = new List<CellVisualBlueprint>(options);
        foreach (CellVisualBlueprint option in options)
        {
            if(option.GetIsAsymmetrical())
            {
                ret.Add(option.GetHorizontallyFlipped());
            }
        }
        return ret;
    }

    private void Update()
    {
        HandleInteraction();
        UpdateEverything();
    }

    private void UpdateEverything()
    {
        while(mainGrid.EmptyCells.Any())
        {
            while(mainGrid.DirtyCells.Any())
            {
                mainGrid.DirtyCells.First().UpdateOptions();
            }
            mainGrid.FillLowestEntropy();
        }
    }

    private void UpdateProgressively()
    {
        if (mainGrid.EmptyCells.Any())
        {
            if (mainGrid.DirtyCells.Any())
            {
                mainGrid.DirtyCells.First().UpdateOptions();
            }
            else
            {
                mainGrid.FillLowestEntropy();
            }
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(mouseRay, out hitInfo))
            {
                TileInteractionBehavior cell = hitInfo.collider.gameObject.GetComponent<TileInteractionBehavior>();
                mainGrid.Designations.ToggleGridpoint(cell.X, cell.Y);
                ResetGridFills();
            }
        }
    }

    private void CreateDisplayTiles()
    {
        GameObject tiles = new GameObject("DisplayTiles");
        tiles.transform.SetParent(MainCanvas.transform, false);
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GameObject obj = CreateDisplayTile(x, y);
                obj.transform.SetParent(tiles.transform, false);
            }
        }
        tiles.transform.position = new Vector3(-(float)Width / 2, -(float)Height / 2);
    }

    private GameObject CreateDisplayTile(int x, int y)
    {
        GameObject obj = Instantiate(DisplayTilePrefab);
        obj.name = x + " " + y;
        obj.transform.position = new Vector3(x + .5f, y + .5f, 0);
        TileVisualBehavior behavior = obj.AddComponent<TileVisualBehavior>();
        behavior.Model = mainGrid.Cells[x, y];
        return obj;
    }
}

[Serializable]
public class CellVisualBlueprint
{
    public Sprite Texture;
    public ConnectionType Left;
    public ConnectionType Right;
    public ConnectionType Up;
    public ConnectionType Down;
    public ConnectionType UpLeft;
    public ConnectionType UpRight;
    public ConnectionType DownLeft;
    public ConnectionType DownRight;

    public bool HorizontallyFlipped { get; set; }

    public bool GetIsAsymmetrical()
    {
        return Left != Right
            || UpLeft != UpRight
            || DownLeft != DownRight;
    }

    public CellVisualBlueprint GetHorizontallyFlipped()
    {
        CellVisualBlueprint ret = new CellVisualBlueprint();
        ret.Texture = Texture;
        ret.Up = Up;
        ret.Down = Down;

        ret.Left = Right;
        ret.Right = Left;
        ret.UpLeft = UpRight;
        ret.UpRight = UpLeft;
        ret.DownLeft = DownRight;
        ret.DownRight = DownLeft;
        ret.HorizontallyFlipped = true;
        return ret;
    }

    public override string ToString()
    {
        return Texture.name;
    }
}

public class TileInteractionBehavior : MonoBehaviour
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class TileVisualBehavior : MonoBehaviour
{
    private Image image;
    private static Vector3 FlippedCoords { get; } = new Vector3(-1, 1, 1); 

    public GridCell Model { get; set; }

    private void Start()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if(Model != null)
        {
            if(Model.FilledWith != null)
            {
                image.sprite = Model.FilledWith.Texture;
                image.transform.localScale = Model.FilledWith.HorizontallyFlipped ? Vector3.one : FlippedCoords;
                //image.material.SetTextureScale("_MainTex", Model.FilledWith.HorizontallyFlipped ? Vector2.one : FlippedCoords);
                //image.material.SetTextureOffset("_MainTex", Model.FilledWith.HorizontallyFlipped ? Vector2.zero : Vector2.right);
            }
            else
            {
                image.sprite = null;
            }
        }
    }
}

public class DesignationsGrid
{
    private readonly int height;
    public ConnectionType[,] Grid;

    public DesignationsGrid(int width, int height)
    {
        this.height = height;
        Grid = new ConnectionType[width, height];
    }

    public void ToggleGridpoint(int x, int y)
    {
        if(y >= height - 2) // Can't toggle the sky
        {
            return;
        }
        Grid[x, y] = Grid[x, y] == ConnectionType.Empty ? ConnectionType.Filled : ConnectionType.Empty;
    }

    public bool IsOptionAllowed(int x, int y, CellVisualBlueprint option)
    {
        return Check(x, y, option.DownRight)
            && Check(x + 1, y, option.DownLeft)
            && Check(x, y + 1, option.UpRight)
            && Check(x + 1, y + 1, option.UpLeft);
    }

    private bool Check(int x, int y, ConnectionType connectionType)
    {
        return connectionType == Grid[x, y];
    }
}

public class MainGrid
{
    private readonly int width;
    private readonly int height;

    public DesignationsGrid Designations { get; }

    public IEnumerable<CellVisualBlueprint> AllOptions { get; }

    public HashSet<GridCell> EmptyCells { get; } = new HashSet<GridCell>();
    public HashSet<GridCell> DirtyCells { get; } = new HashSet<GridCell>();

    public GridCell[,] Cells { get; private set; }

    public MainGrid(int width, int height, IEnumerable<CellVisualBlueprint> allOptions)
    {
        Designations = new DesignationsGrid(width + 1, height + 1);
        AllOptions = allOptions;
        this.width = width;
        this.height = height;
        Cells = CreateCells();
        EmptyCells = GetAllCells();
        DirtyCells = GetAllCells();
    }

    private HashSet<GridCell> GetAllCells()
    {
        HashSet<GridCell> ret = new HashSet<GridCell>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
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

    private GridCell[,] CreateCells()
    {
        GridCell[,] ret = new GridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ret[x, y] = new GridCell(this, x, y);
            }
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ret[x, y].EstablishNeighbors(ret);
            }
        }
        return ret;
    }
}

public class GridCell
{
    private readonly MainGrid main;

    public int X { get; }
    public int Y { get; }

    public IReadOnlyList<CellVisualBlueprint> Options { get; private set; }
    public CellVisualBlueprint FilledWith { get; set; }
    
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
            new Neighbor(this, cells, 1, 0, item => item.Left, item => item.Right),
            new Neighbor(this, cells, -1, 0, item => item.Right, item => item.Left),
            new Neighbor(this, cells, 0, -1, item => item.Up, item => item.Down),
            new Neighbor(this, cells, 0, 1, item => item.Down, item => item.Up),
            new Neighbor(this, cells, 1, -1, item => item.UpLeft, item => item.DownRight),
            new Neighbor(this, cells, -1, -1, item => item.UpRight, item => item.DownLeft),
            new Neighbor(this, cells, 1, 1, item => item.DownLeft, item => item.UpRight),
            new Neighbor(this, cells, -1, 1, item => item.DownRight, item => item.UpLeft)
        };
        this.neighbors = neighbors.Where(item => item.Cell != null).ToList();
    }

    public void Reset()
    {
        FilledWith = null;
        Options = main.AllOptions.Where(DesignationsAllowOption).ToArray();
        if (Options.Count == 0)
        {
            throw new Exception("Impossible from the drop!");
        }
        main.DirtyCells.Add(this);
        main.EmptyCells.Add(this);
    }

    public void UpdateOptions()
    {
        int optionsCount = Options.Count;
        CellVisualBlueprint[] newOptions = Options.Where(OptionIsValid).ToArray();
        if (newOptions.Length == 0)
        {
            throw new Exception("Impossible!");
        }
        Options = newOptions;
        main.DirtyCells.Remove(this);
        if(newOptions.Length != optionsCount)
        {
            foreach (Neighbor neighbor in neighbors)
            {
                neighbor.Cell.SetDirty();
            }
        }
    }

    private bool DesignationsAllowOption(CellVisualBlueprint option)
    {
        return main.Designations.IsOptionAllowed(X, Y, option);
    }

    private void SetDirty()
    {
        if(FilledWith == null)
        {
            main.DirtyCells.Add(this);
        }
    }

    private bool OptionIsValid(CellVisualBlueprint blueprint)
    {
        return neighbors.All(item => item.DoesConnectTo(blueprint));
    }

    internal void FillSelfWithRandomOption()
    {
        CellVisualBlueprint[] optionsArray = Options.ToArray();
        int rand = UnityEngine.Random.Range(0, optionsArray.Length);
        FilledWith = optionsArray[rand];
        main.EmptyCells.Remove(this);
        main.DirtyCells.Remove(this);
        foreach (Neighbor neighbor in neighbors)
        {
            neighbor.Cell.SetDirty();
        }
    }

    private class Neighbor
    {
        public GridCell Cell { get; }
        private readonly Func<CellVisualBlueprint, ConnectionType> selfSelector;
        private readonly Func<CellVisualBlueprint, ConnectionType> neighborSelector;

        public Neighbor(GridCell source,
            GridCell[,] cells, 
            int xOffset, 
            int yOffset,
            Func<CellVisualBlueprint, ConnectionType> selfSelector,
            Func<CellVisualBlueprint, ConnectionType> neighborSelector)
        {
            int xIndex = source.X - xOffset;
            int yIndex = source.Y + yOffset;

            this.selfSelector = selfSelector;
            this.neighborSelector = neighborSelector;

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

        public bool DoesConnectTo(CellVisualBlueprint blueprint)
        {
            if(Cell.FilledWith != null)
            {
                return selfSelector(Cell.FilledWith) == neighborSelector(blueprint);
            }
            return Cell.Options.Any(item => selfSelector(item) == neighborSelector(blueprint));
        }
    }
}

public enum ConnectionType
{
    Empty = 0,
    Filled,
    Struct
}

