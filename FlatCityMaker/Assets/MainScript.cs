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
        mainGrid = new MainGrid(Width, Height, options);
        CreateDisplayTiles();
    }

    private void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            HandleClick(true);
        }
        if(Input.GetMouseButtonUp(1))
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

public class CellVisualBlueprint
{
    public Texture2D Texture => texture;
    public CellDesignation CoreDesignatin => coreDesignation;
    public CellDesignation LeftConnection => leftConnection;
    public CellDesignation RightConnection => rightConnection;
    public CellDesignation UpConnection => upConnection;
    public CellDesignation DownConnection => downConnection;
    [SerializeField]
    private CellDesignation coreDesignation = CellDesignation.Empty;
    [SerializeField]
    private CellDesignation leftConnection = CellDesignation.Empty;
    [SerializeField]
    private CellDesignation upConnection = CellDesignation.Empty;
    [SerializeField]
    private CellDesignation rightConnection = CellDesignation.Empty;
    [SerializeField]
    private CellDesignation downConnection = CellDesignation.Empty;
    [SerializeField]
    private Texture2D texture = null;
}

public class GridCellBehavior : MonoBehaviour
{
    private int designationCycleIndex;
    private static Dictionary<CellDesignation, Color> debugColors = new Dictionary<CellDesignation, Color>()
    {
        {CellDesignation.Empty, Color.white },
        {CellDesignation.Filled, Color.grey },
        {CellDesignation.Ground, Color.black },
        {CellDesignation.Walkway, Color.green }
    };

    private static CellDesignation[] DesignationCycle = new CellDesignation[] { CellDesignation.Empty, CellDesignation.Filled, CellDesignation.Walkway };
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
            Color designationColor = debugColors[Model.Designation];
            mat.SetColor("_Color", designationColor);
        }
    }
}

public class MainGrid
{
    private readonly int width;
    private readonly int height;

    private readonly IReadOnlyDictionary<CellDesignation, List<CellVisualBlueprint>> baseOptions;

    public GridCell[,] Cells { get; private set; }

    public HashSet<GridCell> DirtyCells { get; } = new HashSet<GridCell>();

    public MainGrid(int width, int height, IEnumerable<CellVisualBlueprint> allOptions)
    {
        baseOptions = GetBaseOptions(allOptions);
        this.width = width;
        this.height = height;
        Cells = CreateCells();
    }

    public IEnumerable<CellVisualBlueprint> GetBaseOptions(CellDesignation designation)
    {
        return baseOptions[designation];
    }

    private IReadOnlyDictionary<CellDesignation, List<CellVisualBlueprint>> GetBaseOptions(IEnumerable<CellVisualBlueprint> allOptions)
    {
        Dictionary<CellDesignation, List<CellVisualBlueprint>> ret = new Dictionary<CellDesignation, List<CellVisualBlueprint>>();
        foreach (CellVisualBlueprint item in allOptions)
        {
            if(ret.ContainsKey(item.CoreDesignatin))
            {
                ret[item.CoreDesignatin].Add(item);
            }
            else
            {
                ret.Add(item.CoreDesignatin, new List<CellVisualBlueprint>() { item });
            }
        }
        return ret;
    }

    private GridCell[,] CreateCells()
    {
        GridCell[,] ret = new GridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ret[x, y] = new GridCell(x, y);
            }
        }
        return ret;
    }
}

public class GridCell
{
    public int X { get; }
    public int Y { get; }
    public CellDesignation Designation { get; set; }

    public IEnumerable<CellVisualBlueprint> Blueprints { get; private set; }

    public bool IsDirty { get; private set; }

    public GridCell(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public enum CellDesignation
{
    Empty,
    Filled,
    Ground,
    Walkway
}

