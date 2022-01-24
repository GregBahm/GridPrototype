using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelVisualsManager
{
    private readonly Transform piecesRoot;
    private readonly VisualOptionsByDesignation optionsSource;
    private readonly Dictionary<VisualCell, MeshFilter> voxelObjects = new Dictionary<VisualCell, MeshFilter>();

    public VoxelVisualsManager(VisualOptionsByDesignation optionsSource)
    {
        piecesRoot = new GameObject("Pieces Root").transform;
        this.optionsSource = optionsSource;
    }

    public void UpdateDebugObject(VisualCell component)
    {
        if (voxelObjects.ContainsKey(component))
        {
            MeshFilter filter = voxelObjects[component];
            filter.mesh = component.Contents.Mesh;
            UpdateMeshBounds(filter);
            filter.gameObject.name = GetObjName(component);
            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
            if (component.Contents.Materials != null)
                renderer.materials = component.Contents.Materials.Select(item => new Material(item)).ToArray();
            component.SetMaterialProperties(renderer);
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
            voxelObjects.Add(component, filter);
            obj.transform.SetParent(piecesRoot, true);
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

    internal void DoImmediateUpdate(DesignationCell toggledCell)
    {
        UpdateVoxel(toggledCell);
    }

    private void UpdateVoxel(DesignationCell targetCell)
    {
        foreach (VisualCell component in targetCell.Visuals)
        {
            VoxelDesignation designation = component.GetCurrentDesignation();
            VisualCellOption option = optionsSource.GetOptions(designation).First();
            component.Contents = option;
            UpdateDebugObject(component);
        }
    }
}