using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelVisualsManager
{
    private readonly CityBuildingMain cityMain;
    private readonly Transform piecesRoot;
    private readonly VisualOptionsByDesignation optionsSource;
    private readonly Dictionary<VisualCell, VisualCellGameobjects> voxelObjects = new Dictionary<VisualCell, VisualCellGameobjects>();

    public VoxelVisualsManager(CityBuildingMain cityMain, VisualOptionsByDesignation optionsSource)
    {
        this.cityMain = cityMain;
        piecesRoot = new GameObject("Pieces Root").transform;
        this.anchoringPoints = GetAnchoringPoints();
        this.optionsSource = optionsSource;
    }

    private Dictionary<GroundPoint, PointAnchoring> GetAnchoringPoints()
    {
        cityMain.MainGrid.Points
    }

    public void UpdateDebugObject(VisualCell component)
    {
        VisualCellGameobjects gameObjects;
        if (voxelObjects.ContainsKey(component))
        {
            gameObjects = voxelObjects[component];
            gameObjects.Filter.mesh = component.Contents.Mesh;
            gameObjects.Obj.name = GetObjName(component);
            if (component.Contents.Materials != null)
                gameObjects.Renderer.sharedMaterials = component.Contents.Materials;
        }
        else
        {
            if (component.Contents == null || component.Contents.Mesh == null)
            {
                return;
            }
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = GetObjName(component);
            GameObject.Destroy(obj.GetComponent<BoxCollider>());
            MeshFilter filter = obj.GetComponent<MeshFilter>();
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.sharedMaterials = component.Contents.Materials;
            component.SetMaterialProperties(renderer);
            
            filter.mesh = component.Contents.Mesh;
            obj.transform.position = component.ContentPosition;
            gameObjects = new VisualCellGameobjects(obj, filter, renderer);
            voxelObjects.Add(component, gameObjects);
            obj.transform.SetParent(piecesRoot, true);
        }
        component.UpdateForBaseGridModification(gameObjects.Renderer);
    }

    public void UpdateColumn(GroundQuad column)
    {
        bool yesStrut = false;
        for (int i = cityMain.MainGrid.MaxHeight - 1; i >= 0; i--) // Top to bottom
        {
            VisualCell cell = cityMain.MainGrid.GetVisualCell(column, i);
            VoxelDesignation designation = cell.GetCurrentDesignation();
            VisualCellOptions option = optionsSource.GetOptions(designation);
            VisualCellOption oldOption = cell.Contents;
            if(yesStrut)
            {
                cell.Contents = option.UpStrutOption;
            }
            else
            {
                cell.Contents = option.DefaultOption;
            }
            if(oldOption != cell.Contents)
            {
                UpdateDebugObject(cell);
            }
            yesStrut = cell.Contents.Connections.Down == VoxelConnectionType.BigStrut;
        }
    }

    private string GetObjName(VisualCell component)
    {
        string ret = "(" + component.Quad.ToString() + "), " + component.Height + " ";
        if (component.Contents == null || component.Contents.Mesh == null)
        {
            return ret + " (empty)";
        }
        ret += component.Contents.Mesh.name;
        if (component.Contents.Flipped)
        {
            ret += " flipped";
        }
        if (component.Contents.Rotations > 0)
        {
            ret += " " + component.Contents.Rotations.ToString() + " rotations";
        }
        return ret;
    }

    internal void UpdateForBaseGridModification()
    {
        foreach (KeyValuePair<VisualCell, VisualCellGameobjects> item in voxelObjects)
        {
            item.Key.UpdateForBaseGridModification(item.Value.Renderer);
        }
    }

    private class VisualCellGameobjects
    {
        public GameObject Obj { get; }
        public MeshFilter Filter { get; }
        public MeshRenderer Renderer { get; }

        public VisualCellGameobjects(GameObject obj, MeshFilter filter, MeshRenderer renderer)
        {
            Obj = obj;
            Filter = filter;
            Renderer = renderer;
        }
    }
}

public class PointAnchoringTable
{
    private readonly Dictionary<GroundPoint, PointAnchoring> table;

    public PointAnchoringTable(MainGrid grid)
    {
        table = new Dictionary<GroundPoint, PointAnchoring>();
        foreach (GroundPoint point in grid.Points)
        {
            GroundPoint[] connections = point.DirectConnections.ToArray();
            PointAnchoring anchoring = GetAnchoring(point, connections);
            table.Add(point, anchoring);
        }
    }

    private PointAnchoring GetAnchoring(GroundPoint point, GroundPoint[] connections)
    {
        int half = connections.Length / 2;
        for (int i = 0; i < half; i++)
        {
            Vector2 norm = (connections[i].Position - point.Position).normalized;
            if((i + 2) < connections.Length - 1)
            {
                Vector2 otherNorm = (connections[i + 2].Position - point.Position).normalized;
                norm = (norm + otherNorm) / 2;
            }
        }

    }

    private class PointAnchoring
    {
        public Vector2 BasePos { get; set; }
        public Vector2 XNormal { get; set; }
        public Vector2 ZNormal { get; set; }
    }
}