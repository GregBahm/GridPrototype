using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public int Width;
    public int Height;
    public Material BaseMat;

    [SerializeField]
    private CellVisualBlueprint[] options;

    private MainGrid mainGrid;

    void Start()
    {
        IEnumerable<CellVisualBlueprint> allOptions = GetSymmetricalOptions();
        mainGrid = new MainGrid(Width, Height, options);
        
        CreateDisplayTiles();
        ResetGridFills();
    }

    private void ResetGridFills()
    {
        foreach (var item in mainGrid.Cells)
        {
            item.Reset();
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
            HandleClick(true);
        }
        if (Input.GetMouseButtonUp(1))
        {
            HandleClick(false);
        }
    }

    private void HandleClick(bool leftClick)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(mouseRay, out hitInfo))
        {
            GridCellBehavior cell = hitInfo.collider.gameObject.GetComponent<GridCellBehavior>();
            if(leftClick)
            {
                cell.CycleDesignation(1);
            }
            else
            {
                cell.CycleDesignation(-1);
            }
            ResetGridFills();
        }
    }

    private void CreateDisplayTiles()
    {
        GameObject tiles = new GameObject("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GameObject obj =CreateDisplayTile(x, y);
                obj.transform.parent = tiles.transform;
            }
        }
        tiles.transform.position = new Vector3(-(float)Width / 2, -(float)Height / 2);
    }

    private GameObject CreateDisplayTile(int x, int y)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = x + " " + y;
        obj.transform.position = new Vector3(x + .5f, y + .5f, 0);
        obj.GetComponent<MeshRenderer>().material = new Material(BaseMat);
        GridCellBehavior behavior = obj.AddComponent<GridCellBehavior>();
        behavior.Model = mainGrid.Cells[x, y];
        return obj;
    }
}

[Serializable]
public class CellVisualBlueprint
{
    public Texture2D Texture;
    public CoreDesignation Core;
    public ConnectionType Left;
    public ConnectionType Right;
    public ConnectionType Up;
    public ConnectionType Down;
    public ConnectionType UpLeft;
    public ConnectionType UpRight;
    public ConnectionType DownLeft;
    public ConnectionType DownRight;

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
        ret.Core = Core;
        ret.Up = Up;
        ret.Down = Down;

        ret.Left = Right;
        ret.Right = Left;
        ret.UpLeft = UpRight;
        ret.UpRight = UpLeft;
        ret.DownLeft = DownRight;
        ret.DownRight = DownLeft;
        return ret;
    }
}

public class GridCellBehavior : MonoBehaviour
{
    private int designationCycleIndex;
    private static Dictionary<CoreDesignation, Color> debugColors = new Dictionary<CoreDesignation, Color>()
    {
        {CoreDesignation.Empty, Color.white },
        {CoreDesignation.Filled, Color.grey },
    };

    private static CoreDesignation[] DesignationCycle = new CoreDesignation[] { CoreDesignation.Empty, CoreDesignation.Filled };
    private Material mat;

    public GridCell Model { get; set; }

    internal void CycleDesignation(int by)
    {
        designationCycleIndex = (designationCycleIndex + DesignationCycle.Length + by) % DesignationCycle.Length;
        Model.Designation = DesignationCycle[designationCycleIndex];
    }

    private void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        if(Model != null)
        {
            //Color designationColor = debugColors[Model.Designation];
            //mat.SetColor("_Color", designationColor);
            if(Model.FilledWith != null)
            {
                mat.SetTexture("_MainTex", Model.FilledWith.Texture);
            }
            else
            {
                mat.SetTexture("_MainTex", null);
            }
        }
    }
}

public class MainGrid
{
    private readonly int width;
    private readonly int height;

    public IEnumerable<CellVisualBlueprint> AllOptions { get; }

    public HashSet<GridCell> EmptyCells { get; } = new HashSet<GridCell>();
    public HashSet<GridCell> DirtyCells { get; } = new HashSet<GridCell>();

    public GridCell[,] Cells { get; private set; }

    public MainGrid(int width, int height, IEnumerable<CellVisualBlueprint> allOptions)
    {
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

    public CoreDesignation Designation { get; set; }

    public IReadOnlyList<CellVisualBlueprint> Options { get; private set; }
    public CellVisualBlueprint FilledWith { get; private set; }
    
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
        Options = main.AllOptions.Where(item => item.Core == Designation).ToArray();
        main.DirtyCells.Add(this);
        main.EmptyCells.Add(this);
    }

    public void UpdateOptions()
    {
        int optionsCount = Options.Count;
        Options = Options.Where(OptionIsValid).ToArray();
        main.DirtyCells.Remove(this);
        if(Options.Count != optionsCount)
        {
            foreach (Neighbor neighbor in neighbors)
            {
                neighbor.Cell.SetDirty();
            }
        }
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
        return Designation == blueprint.Core
            && neighbors.All(item => item.DoesConnectTo(blueprint));
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
            int xIndex = source.X + xOffset;
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

public enum CoreDesignation
{
    Empty,
    Filled,
}
public enum ConnectionType
{
    Empty,
    Filled,
    Struct
}

