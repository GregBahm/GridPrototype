﻿using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelVisualsManager
{
    private readonly Transform piecesRoot;
    private readonly Material voxelDisplayMat;
    private readonly OptionsByDesignation optionsSource;
    private readonly Dictionary<VoxelVisualComponent, MeshFilter> debugObjects = new Dictionary<VoxelVisualComponent, MeshFilter>();

    public VoxelVisualsManager(Material voxelDisplayMat, OptionsByDesignation optionsSource)
    {
        piecesRoot = new GameObject("Pieces Root").transform;
        this.voxelDisplayMat = voxelDisplayMat;
        this.optionsSource = optionsSource;
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

    public void UpdateDebugObject(VoxelVisualComponent component)
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

            //TODO: Delete this when you're done debugging
            VoxelVisualDebugger debugger = obj.AddComponent<VoxelVisualDebugger>();
            debugger.Component = component;
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

    internal void DoImmediateUpdate(VoxelCell toggledCell)
    {
        UpdateVoxel(toggledCell);
    }

    private void UpdateVoxel(VoxelCell targetCell)
    {
        foreach (VoxelVisualComponent component in targetCell.Visuals.Components)
        {
            VoxelDesignation designation = component.GetCurrentDesignation();
            VoxelVisualOption option = optionsSource.GetOptions(designation).First();
            component.Contents = option;
            UpdateDebugObject(component);
        }
    }

    private class VoxelVisualDebugger : MonoBehaviour
    {
        public bool DoDebug;
        public VoxelVisualComponent Component;

        private static Color PositiveZColor = new Color(0, 0, 1f);
        private static Color NegativeZColor = new Color(0, 0, .5f);
        private static Color PositiveXColor = new Color(1f, 0, 0);
        private static Color NegativeXColor = new Color(.5f, 0, 0);

        private void Update()
        {
            if(DoDebug)
            {
                Debug.DrawLine(Component.VisualCenter, Component.Neighbors.Forward.VisualCenter, PositiveZColor);
                Debug.DrawLine(Component.VisualCenter, Component.Neighbors.Back.VisualCenter, NegativeZColor);
                Debug.DrawLine(Component.VisualCenter, Component.Neighbors.Left.VisualCenter, PositiveXColor);
                Debug.DrawLine(Component.VisualCenter, Component.Neighbors.Right.VisualCenter, NegativeXColor);
            }
        }
    }
}