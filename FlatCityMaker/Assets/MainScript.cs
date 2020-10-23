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

[Serializable]
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
    };

    private static CellDesignation[] DesignationCycle = new CellDesignation[] { CellDesignation.Empty, CellDesignation.Filled };
    private Material mat;

    public IGridCell Model { get; set; }

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

    public IEnumerable<CellVisualBlueprint> AllOptions { get; }

    public IGridCell[,] Cells { get; private set; }

    public MainGrid(int width, int height, IEnumerable<CellVisualBlueprint> allOptions)
    {
        AllOptions = allOptions;
        this.width = width;
        this.height = height;
        Cells = CreateCells();
    }

    private IGridCell[,] CreateCells()
    {
        IGridCell[,] ret = new IGridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ret[x, y] = new GridCell(x, y);
            }
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ret[x, y].SetNeighbors(ret);
            }
        }
        return ret;
    }
}

public interface IGridCell
{
    CellDesignation Designation { get; set; }

    void SetNeighbors(IGridCell[,] grid);
}

public class OffGridCell : IGridCell
{
    public static OffGridCell GroundCell = new OffGridCell(CellDesignation.Ground);
    public static OffGridCell SkyCell = new OffGridCell(CellDesignation.Empty);

    public CellDesignation Designation { get; set; }

    public OffGridCell(CellDesignation designation)
    {
        Designation = designation;
    }

    public void SetNeighbors(IGridCell[,] grid)
    { }
}

public class GridCell : IGridCell
{
    private readonly MainGrid main;

    public int X { get; }
    public int Y { get; }

    public IGridCell LeftNeighbor { get; private set; }
    public IGridCell RightNeighbor { get; private set; }
    public IGridCell UpNeighbor { get; private set; }
    public IGridCell DownNeighbor { get; private set; }

    public CellDesignation Designation { get; set; }

    private IEnumerable<CellVisualBlueprint> options;

    public GridCell(MainGrid main, int x, int y)
    {
        this.main = main;
        X = x;
        Y = y;
    }

    public void SetNeighbors(IGridCell[,] cells)
    {
        int width = cells.GetLength(0);
        int height = cells.GetLength(1);
        LeftNeighbor = X > 0 ? cells[X - 1, Y] : OffGridCell.SkyCell;
        RightNeighbor = X < width - 1 ? cells[X + 1, Y] : OffGridCell.SkyCell;
        DownNeighbor = Y > 0 ? cells[X, Y - 1] : OffGridCell.GroundCell;
        UpNeighbor = Y < height - 1 ? cells[X, Y + 1] : OffGridCell.SkyCell;
    }

    public void UpdateOptions()
    {
        options = main.AllOptions.Where(OptionIsValid).ToArray();
    }

    private bool OptionIsValid(CellVisualBlueprint blueprint)
    {
        return Designation == blueprint.CoreDesignatin
            && UpNeighbor.Designation == blueprint.UpConnection
            && DownNeighbor.Designation == blueprint.DownConnection
            && LeftNeighbor.Designation == blueprint.LeftConnection
            && RightNeighbor.Designation == blueprint.RightConnection;
    }
}

public enum CellDesignation
{
    Empty,
    Filled,
    Ground,
    SupportPillar
}

