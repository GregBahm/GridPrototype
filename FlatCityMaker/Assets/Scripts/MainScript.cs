using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TileDefinition;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public int Width;
    public int Height;
    public GameObject DisplayTilePrefab;
    public RectTransform DisplayTilesTransform;

    public Tile SkyTile;

    [SerializeField]
    private Tile[] options;

    public TileConnectionType[] FilledConnectionTypes;
    public ReadOnlyCollection<TileInteractionBehavior> InteractionCells { get; private set; }
    public IEnumerable<TileVisualBehavior> VisualTiles { get; private set; }

    public MainGrid MainGrid { get; private set; }

    void Start()
    {
        IEnumerable<Tile> allOptions = GetSymmetricalOptions();
        DesignationsGrid designations = new DesignationsGrid(Width, Height, FilledConnectionTypes);
        MainGrid = new MainGrid(Width, Height, allOptions, designations);

        VisualTiles = CreateDisplayTiles();
        InteractionCells = CreateInteractionTiles().AsReadOnly();
    }

    private List<TileInteractionBehavior> CreateInteractionTiles()
    {
        List<TileInteractionBehavior> ret = new List<TileInteractionBehavior>();
        GameObject tiles = new GameObject("InteractionTiles");
        for (int x = 0; x < Width + 1; x++)
        {
            for (int y = 0; y < Height + 1; y++)
            {
                TileInteractionBehavior obj = CreateInteractionTile(x, y);
                obj.transform.parent = tiles.transform;
                ret.Add(obj);
            }
        }
        tiles.transform.position = new Vector3(-(float)Width / 2, -(float)Height / 2);
        return ret;
    }

    private IEnumerable<GridCell> GetConnectedCells(int x, int y)
    {
        x--;
        y--;
        if (x > 0 && y > 0)
            yield return MainGrid.Cells[x, y];
        if (x < Width - 2 && y > 0)
            yield return MainGrid.Cells[x + 1, y];
        if (x > 0 && y < Height - 1)
            yield return MainGrid.Cells[x, y + 1];
        if (x < Width - 1 && y < Height - 1)
            yield return MainGrid.Cells[x + 1, y + 1];
    }

    private TileInteractionBehavior CreateInteractionTile(int x, int y)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = x + " " + y;
        obj.transform.position = new Vector3(x, y, 0);
        Destroy(obj.GetComponent<MeshRenderer>());
        TileInteractionBehavior behavior = obj.AddComponent<TileInteractionBehavior>();
        behavior.X = x;
        behavior.Y = y;
        behavior.ConnectedCells = GetConnectedCells(x, y).ToArray();
        return behavior;
    }

    private IEnumerable<Tile> GetSymmetricalOptions()
    {
        List<Tile> ret = new List<Tile>();
        foreach (Tile option in options)
        {
            ret.Add(option);
            if (option.GetIsAsymmetrical())
            {
                ret.Add(option.GetHorizontallyFlipped());
            }
        }
        return ret;
    }

    public bool Test;
    public bool TestB;

    private void Update()
    {
        if(Test)
        {
            Test = false;
            ToggleCell(InteractionCells[InteractionCells.Count / 2]);
        }
        if (TestB)
        {
            TestB = false;
            ToggleCell(InteractionCells[InteractionCells.Count / 2 + 21]);
        }
        HandleInteraction();
        UpdateProgressive();
    }

    private void UpdateProgressive()
    {
        if(MainGrid.DirtyCells.Any())
        {
            MainGrid.DirtyCells.OrderByDescending(item => item.Options.Count).First().UpdateOptions();
            //mainGrid.DirtyCells.First().UpdateOptions();
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
                ToggleCell(cell);
            }
        }
    }

    private void ToggleCell(TileInteractionBehavior cell)
    {
        MainGrid.RefreshedCells.Clear();
        MainGrid.Designations.ToggleGridpoint(cell.X, cell.Y);
        UpdateCells(cell.ConnectedCells);
    }

    private void UpdateCells(IEnumerable<GridCell> connectedCells)
    {
        foreach (GridCell cell in connectedCells)
        {
            cell.ResetDesignationOptions();
        }
        //while(mainGrid.DirtyCells.Any())
        //{
        //    mainGrid.DirtyCells.First().UpdateOptions();
        //}
    }


    private IEnumerable<TileVisualBehavior> CreateDisplayTiles()
    {
        List<TileVisualBehavior> ret = new List<TileVisualBehavior>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                TileVisualBehavior behavior = CreateDisplayTile(x, y);
                behavior.transform.SetParent(DisplayTilesTransform, false);
                ret.Add(behavior);
            }
        }
        DisplayTilesTransform.position = new Vector3(-(float)Width / 2, -(float)Height / 2);
        return ret;
    }

    private TileVisualBehavior CreateDisplayTile(int x, int y)
    {
        GameObject obj = Instantiate(DisplayTilePrefab);
        obj.name = x + " " + y;
        obj.transform.position = new Vector3(x + .5f, y + .5f, 0);
        TileVisualBehavior behavior = obj.AddComponent<TileVisualBehavior>();
        behavior.Model = MainGrid.Cells[x, y];
        return behavior;
    }
}