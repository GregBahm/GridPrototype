using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelVisualsManager
{
    private readonly Material voxelDisplayMat;
    private readonly VoxelVisualOption[] allOptions;
    private readonly Dictionary<string, VoxelVisualOption> optionsByDesignationKey;
    private readonly Dictionary<VoxelCell, VoxelVisuals> visuals;
    private readonly Dictionary<VoxelVisualComponent, MeshFilter> debugObjects = new Dictionary<VoxelVisualComponent, MeshFilter>();

    public VoxelVisualsManager(VoxelBlueprint[] blueprints, MainGrid grid, Material voxelDisplayMat)
    {
        allOptions = GetAllOptions(blueprints).ToArray();
        optionsByDesignationKey = allOptions.ToDictionary(item => item.GetDesignationKey(), item => item);
        visuals = CreateVisuals(grid);
        this.voxelDisplayMat = voxelDisplayMat;
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
            VoxelVisualOption option = optionsByDesignationKey[designation.ToString()];

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
        if(debugObjects.ContainsKey(component))
        {
            debugObjects[component].mesh = component.Contents.Mesh;
        }
        else
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
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
        }
    }
}