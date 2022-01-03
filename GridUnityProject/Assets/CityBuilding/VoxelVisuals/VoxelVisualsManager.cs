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
    private readonly VisualOptionsByDesignation optionsSource;
    private readonly Dictionary<VisualCell, MeshFilter> debugObjects = new Dictionary<VisualCell, MeshFilter>();

    public VoxelVisualsManager(Material voxelDisplayMat, VisualOptionsByDesignation optionsSource)
    {
        piecesRoot = new GameObject("Pieces Root").transform;
        this.voxelDisplayMat = voxelDisplayMat;
        this.optionsSource = optionsSource;
    }

    private List<Tuple<Material, VisualCell>> debugMats = new List<Tuple<Material, VisualCell>>();

    public void ConstantlyUpdateComponentTransforms()
    {
        //TODO: Remove this when you're done iterating on the shaders
        foreach (var item in debugMats)
        {
            item.Item2.SetComponentTransform(item.Item1);
        }
    }

    public void UpdateDebugObject(VisualCell component)
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
            debugMats.Add(new Tuple<Material, VisualCell>(mat, component));
            
            filter.mesh = component.Contents.Mesh;
            obj.transform.position = component.ContentPosition;
            debugObjects.Add(component, filter);
            obj.transform.SetParent(piecesRoot, false);

            //TODO: Delete this when you're done debugging
            VoxelVisualDebugger debugger = obj.AddComponent<VoxelVisualDebugger>();
            debugger.Component = component;
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

    private class VoxelVisualDebugger : MonoBehaviour
    {
        public bool DoDebug;
        public VisualCell Component;

        private static Color PositiveZColor = new Color(0, 0, 1f);
        private static Color NegativeZColor = new Color(0, 0, .5f);
        private static Color PositiveXColor = new Color(1f, 0, 0);
        private static Color NegativeXColor = new Color(.5f, 0, 0);

        private void Update()
        {

            if(DoDebug)
            {
                Debug.DrawLine(Component.ContentPosition, Component.Neighbors.Forward.ContentPosition, PositiveZColor);
                Debug.DrawLine(Component.ContentPosition, Component.Neighbors.Back.ContentPosition, NegativeZColor);
                Debug.DrawLine(Component.ContentPosition, Component.Neighbors.Left.ContentPosition, PositiveXColor);
                Debug.DrawLine(Component.ContentPosition, Component.Neighbors.Right.ContentPosition, NegativeXColor);
            }
        }
    }
}