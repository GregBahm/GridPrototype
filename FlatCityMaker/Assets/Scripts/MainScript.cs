using System;
using System.Collections.Generic;
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

    private MainGrid mainGrid;

    void Start()
    {
        IEnumerable<Tile> allOptions = GetSymmetricalOptions();
        DesignationsGrid designations = new DesignationsGrid(Width, Height, FilledConnectionTypes);
        mainGrid = new MainGrid(Width, Height, allOptions, designations);
        
        CreateDisplayTiles();
        CreateInteractionTiles();
        FillAllWithSky();
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
        behavior.ConnectedCells = GetConnectedCells(x, y).ToArray();
        return obj;
    }

    private IEnumerable<GridCell> GetConnectedCells(int x, int y)
    {
        x--;
        y--;
        if (x > 0 && y > 0)
            yield return mainGrid.Cells[x, y];
        if(x < Width - 2 && y > 0)
            yield return mainGrid.Cells[x + 1, y];
        if(x > 0 && y < Height - 1)
            yield return mainGrid.Cells[x, y + 1];
        if(x < Width - 1 && y < Height - 1)
            yield return mainGrid.Cells[x + 1, y + 1];
    }
    private void ResetAffectedCells(IEnumerable<GridCell> cells)
    {
        foreach (GridCell cell in cells)
        {
            cell.Reset();
        }
    }

    private void FillAllWithSky()
    {
        foreach (var item in mainGrid.Cells)
        {
            item.FilledWith = SkyTile;
        }
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

    private void Update()
    {
        HandleInteraction();
        UpdateProgressively();
    }

    private void UpdateEverything()
    {
        while(mainGrid.EmptyCells.Any())
        {
            mainGrid.TryFillLowestEntropy();
        }
    }

    private void UpdateProgressively()
    {
        if (mainGrid.EmptyCells.Any())
        {
            mainGrid.TryFillLowestEntropy();
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
                mainGrid.StartNewSolve();
                ResetAffectedCells(cell.ConnectedCells);
            }
        }
    }

    private void CreateDisplayTiles()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GameObject obj = CreateDisplayTile(x, y);
                obj.transform.SetParent(DisplayTilesTransform, false);
            }
        }
        DisplayTilesTransform.position = new Vector3(-(float)Width / 2, -(float)Height / 2);
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

