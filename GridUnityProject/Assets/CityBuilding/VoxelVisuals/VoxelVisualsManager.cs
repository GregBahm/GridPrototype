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
        this.optionsSource = optionsSource;
    }

    public void UpdateDebugObject(VisualCell component)
    {
        if (voxelObjects.ContainsKey(component))
        {
            VisualCellGameobjects gameObjects = voxelObjects[component];
            gameObjects.Filter.mesh = component.Contents.Mesh;
            UpdateMeshBounds(gameObjects.Filter);
            gameObjects.Obj.name = GetObjName(component);
            if (component.Contents.Materials != null)
                gameObjects.Renderer.materials = component.Contents.Materials.Select(item => new Material(item)).ToArray();

            component.UpdateForBaseGridModification(gameObjects.Renderer);
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
            UpdateMeshBounds(filter);
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.materials = component.Contents.Materials.Select(item => new Material(item)).ToArray();
            component.SetMaterialProperties(renderer);
            
            filter.mesh = component.Contents.Mesh;
            obj.transform.position = component.ContentPosition;
            VisualCellGameobjects gameObjects = new VisualCellGameobjects(obj, filter, renderer);
            voxelObjects.Add(component, gameObjects);
            obj.transform.SetParent(piecesRoot, true);
        }
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

    private static readonly Bounds ComponentBounds = new Bounds(Vector3.zero, Vector3.one* 2);

    private void UpdateMeshBounds(MeshFilter filter)
    {
        if(filter.mesh != null)
        {
            filter.mesh.bounds = ComponentBounds;
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