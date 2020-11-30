using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelVisualsManager
{
    private readonly OptionsByDesignation optionsManager;
    private readonly Transform piecesRoot;
    private readonly Material voxelDisplayMat;
    private readonly Dictionary<VoxelVisualComponent, MeshFilter> debugObjects = new Dictionary<VoxelVisualComponent, MeshFilter>();

    public VoxelVisualsManager(OptionsByDesignation optionsManager, MainGrid grid, Material voxelDisplayMat)
    {
        this.optionsManager = optionsManager;
        piecesRoot = new GameObject("Pieces Root").transform;
        this.voxelDisplayMat = voxelDisplayMat;
    }

    internal void UpdateVoxels(VoxelCell toggledCell)
    {
        UpdateVoxel(toggledCell);
        var connected = toggledCell.GetConnectedCells().ToArray();
        foreach (VoxelCell cell in connected)
        {
            UpdateVoxel(cell);
        }
    }

    private void UpdateVoxel(VoxelCell targetCell)
    {
        foreach (VoxelVisualComponent component in targetCell.Visuals.Components)
        {
            VoxelDesignation designation = component.GetCurrentDesignation();
            VoxelVisualOption option = optionsManager.GetOptions(designation).First();
            component.Contents = option;
            UpdateDebugObject(component);
        }
    }

    private List<Tuple<Material, VoxelVisualComponent>> debugMats = new List<Tuple<Material, VoxelVisualComponent>>();

    public void ConstantlyUpdateComponentTransforms()
    {
        //TODO: Remove this when you're done iterating on the shaders
        foreach (var item in debugMats)
        {
            item.Item2.SetComponentTransform(item.Item1);
        }
    }

    private void UpdateDebugObject(VoxelVisualComponent component)
    {
        if (debugObjects.ContainsKey(component))
        {
            debugObjects[component].mesh = component.Contents.Mesh;
            debugObjects[component].gameObject.name = GetObjName(component);
            component.SetComponentTransform(debugObjects[component].GetComponent<MeshRenderer>().material);
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
            Material mat = new Material(voxelDisplayMat);
            component.SetComponentTransform(mat);
            renderer.material = mat;
            debugMats.Add(new Tuple<Material, VoxelVisualComponent>(mat, component));

            filter.mesh = component.Contents.Mesh;
            obj.transform.position = component.ContentPosition;
            debugObjects.Add(component, filter);
            obj.transform.SetParent(piecesRoot, false);
        }
    }

    private string GetObjName(VoxelVisualComponent component)
    {
        string ret = component.Core.ToString();
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
}