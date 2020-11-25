using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelVisualsManager
{
    private readonly Transform piecesRoot;
    private readonly Material voxelDisplayMat;
    private readonly VoxelVisualOption[] allOptions;
    private readonly Dictionary<string, IEnumerable<VoxelVisualOption>> optionsByDesignationKey;
    private readonly Dictionary<VoxelCell, VoxelVisuals> visuals;
    private readonly Dictionary<VoxelVisualComponent, MeshFilter> debugObjects = new Dictionary<VoxelVisualComponent, MeshFilter>();

    public VoxelVisualsManager(VoxelBlueprint[] blueprints, MainGrid grid, Material voxelDisplayMat)
    {
        piecesRoot = new GameObject("Pieces Root").transform;
        allOptions = GetAllOptions(blueprints).ToArray();
        optionsByDesignationKey = GetOptionsByDesignationKey();
        visuals = CreateVisuals(grid);
        this.voxelDisplayMat = voxelDisplayMat;
    }

    private Dictionary<string, IEnumerable<VoxelVisualOption>> GetOptionsByDesignationKey()
    {
        return allOptions.GroupBy(item => item.GetDesignationKey())
            .ToDictionary(item => item.Key, item => (IEnumerable<VoxelVisualOption>)item);
    }

    private Dictionary<VoxelCell, VoxelVisuals> CreateVisuals(MainGrid grid)
    {
        Dictionary<VoxelCell, VoxelVisuals> ret = new Dictionary<VoxelCell, VoxelVisuals>();
        foreach (GroundPoint point in grid.Points)
        {
            for (int i = 0; i < MainGrid.VoxelHeight - 1; i++)
            {
                VoxelCell cell = point.Voxels[i];
                VoxelVisuals visuals = new VoxelVisuals(cell);
                ret.Add(cell, visuals);
            }
        }
        return ret;
    }

    private IEnumerable<VoxelVisualOption> GetAllOptions(VoxelBlueprint[] blueprints)
    {
        foreach (VoxelBlueprint blueprint in blueprints)
        {
            IEnumerable<VoxelVisualOption> options = blueprint.GenerateVisualOptions();
            foreach (VoxelVisualOption option in options)
            {
                yield return option;
            }
        }
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
        VoxelVisuals visual = visuals[targetCell];
        foreach (VoxelVisualComponent component in visual.Components)
        {
            VoxelDesignation designation = component.GetCurrentDesignation();
            VoxelVisualOption option = optionsByDesignationKey[designation.ToString()].First();
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