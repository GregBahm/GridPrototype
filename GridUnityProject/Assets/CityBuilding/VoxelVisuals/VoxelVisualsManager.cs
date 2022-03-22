using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelVisualsManager
{
    private readonly CityBuildingMain cityMain;
    private readonly VisualOptionsByDesignation optionsSource;

    private readonly Dictionary<VisualCell, VisualCellOption> toRemove;
    private readonly Dictionary<VisualCell, VisualCellOption> toAdd;
    private Dictionary<Mesh, ProceduralMeshRenderer> renderers;

    public VoxelVisualsManager(CityBuildingMain cityMain, VisualOptionsByDesignation optionsSource)
    {
        this.cityMain = cityMain;
        this.optionsSource = optionsSource;
        toRemove = new Dictionary<VisualCell, VisualCellOption>();
        toAdd = new Dictionary<VisualCell, VisualCellOption>();
        renderers = GetRenderers(optionsSource);
        VisualCell.ContentsChanged += OnVoxelOptionChange;
    }

    public void UpdateColumn(GroundQuad column)
    {
        bool yesStrut = false;
        for (int i = cityMain.MainGrid.MaxHeight - 1; i >= 0; i--) // Top to bottom
        {
            VisualCell cell = cityMain.MainGrid.GetVisualCell(column, i);
            VoxelDesignation designation = cell.GetCurrentDesignation();
            VisualCellOptions option = optionsSource.GetOptions(designation);
            if (yesStrut)
            {
                cell.Contents = option.UpStrutOption;
            }
            else
            {
                cell.Contents = option.DefaultOption;
            }
            yesStrut = cell.Contents.Connections.Down == VoxelConnectionType.BigStrut;
        }
    }

    public void UpdateForBaseGridModification()
    {
        // TODO: Implement this. It just needs to roll through and update all anchors for all renderers
    }

    private Dictionary<Mesh, ProceduralMeshRenderer> GetRenderers(VisualOptionsByDesignation optionsSource)
    {
        Dictionary<Mesh, ProceduralMeshRenderer> ret = new Dictionary<Mesh, ProceduralMeshRenderer>();
        IEnumerable<VoxelBlueprint> blueprints = optionsSource.Blueprints.Where(item => item.ArtContent != null);
        foreach (VoxelBlueprint item in blueprints)
        {
            ProceduralMeshRenderer renderer = new ProceduralMeshRenderer(item.ArtContent, item.Materials);
            ret.Add(item.ArtContent, renderer);
        }
        return ret;
    }

    private void OnVoxelOptionChange(object sender, VisualCellChangedEventArg args)
    {
        RegisterRemoval(args);
        RegisterAdd(args);
    }

    public void Update()
    {
        ProcessAllToRemoves();
        ProcessAllToAdds();
        UpdateBuffers();
        Render();
    }

    private void Render()
    {
        foreach (ProceduralMeshRenderer item in renderers.Values.Where(item => item.CellsToRender > 0))
        {
            item.Render();
        }
    }

    private void UpdateBuffers()
    {
        foreach (ProceduralMeshRenderer item in renderers.Values.Where(item => item.IsDirty))
        {
            item.UpdateBuffers();
        }
    }

    private void ProcessAllToRemoves()
    {
        foreach (var item in toRemove)
        {
            ProceduralMeshRenderer renderer = renderers[item.Value.Mesh];
            renderer.Remove(item.Key);
        }
        toRemove.Clear();
    }

    private void ProcessAllToAdds()
    {
        foreach (var item in toAdd)
        {
            ProceduralMeshRenderer renderer = renderers[item.Value.Mesh];
            renderer.Add(item.Key);
        }
        toAdd.Clear();
    }

    private void RegisterRemoval(VisualCellChangedEventArg args)
    {
        if (args.OldOption == null || args.OldOption.Mesh == null)
            return;
        if (!toRemove.ContainsKey(args.Cell)) // Discard each change after the first as they were never applied
        {
            toRemove.Add(args.Cell, args.OldOption);
        }
    }

    private void RegisterAdd(VisualCellChangedEventArg args)
    {
        if (args.Cell.Contents.Mesh == null)
            return;
        if (toAdd.ContainsKey(args.Cell))
        {
            toAdd[args.Cell] = args.Cell.Contents;
        }
        else
        {
            toAdd.Add(args.Cell, args.Cell.Contents);
        }
    }

    public void Dispose()
    {
        foreach (ProceduralMeshRenderer item in renderers.Values)
        {
            item.Dispose();
        }
    }
}

public class VisualCellChangedEventArg : EventArgs
{
    public VisualCell Cell { get; }
    public VisualCellOption OldOption { get; }

    public VisualCellChangedEventArg(VisualCell cell, VisualCellOption oldOption)
    {
        Cell = cell;
        OldOption = oldOption;
    }
}

class ProceduralMeshRenderer
{
    private static Bounds Bounds { get; } = new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f));

    public Mesh Mesh { get; }
    private readonly Material[] materials;

    public bool IsDirty { get; private set; }

    private readonly HashSet<VisualCell> cellsToRender;
    public int CellsToRender { get { return cellsToRender.Count; } }

    private ComputeBuffer renderDataBuffer;
    private int renderBufferLength = 1024; // TODO: Lower this and then make it dynamic
    private ComputeBuffer argsBuffer;
    private const int PositionsBufferStride = VoxelRenderData.Stride;

    public ProceduralMeshRenderer(Mesh mesh, Material[] materials)
    {
        Mesh = mesh;
        this.materials = materials.Select(item => new Material(item)).ToArray();
        renderDataBuffer = new ComputeBuffer(renderBufferLength, PositionsBufferStride);
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        cellsToRender = new HashSet<VisualCell>();
    }

    public void Dispose()
    {
        renderDataBuffer.Dispose();
        renderDataBuffer = null;
        argsBuffer.Dispose();
        argsBuffer = null;
    }

    public void Add(VisualCell cell)
    {
        cellsToRender.Add(cell);
        IsDirty = true;
    }

    public void Remove(VisualCell cell)
    {
        cellsToRender.Remove(cell);
        IsDirty = true;
    }

    public void UpdateBuffers()
    {
        UpdateArgsBuffer();
        UpdatePositionsBuffer();
    }

    private void UpdatePositionsBuffer()
    {
        VoxelRenderData[] rendereData = cellsToRender.Select(item => item.GetRenderData()).ToArray();
        if (rendereData.Length > renderBufferLength)
        {
            throw new NotImplementedException("Need to handle growth of render data buffers");
        }
        renderDataBuffer.SetData(rendereData);
    }

    private void UpdateArgsBuffer()
    {
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = Mesh.GetIndexCount(0);
        args[1] = (uint)cellsToRender.Count;
        args[2] = Mesh.GetIndexStart(0);
        args[3] = Mesh.GetBaseVertex(0);
        argsBuffer.SetData(args);
    }

    public void Render()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            mat.SetBuffer("_RenderDataBuffer", renderDataBuffer);
            Graphics.DrawMeshInstancedIndirect(Mesh, i, mat, Bounds, argsBuffer);
        }
    }
}

public struct VoxelRenderData
{
    public const int Stride = sizeof(float) * 2 * 4  // Anchors
        + sizeof(float); // FlipNormal

    public Vector2 AnchorA { get; }
    public Vector2 AnchorB { get; }
    public Vector2 AnchorC { get; }
    public Vector2 AnchorD { get; }
    public float FlipNormal { get; }

    public VoxelRenderData(Vector2 anchorA,
        Vector2 anchorB,
        Vector2 anchorC,
        Vector2 anchorD,
        float flipNormal)
    {
        AnchorA = anchorA;
        AnchorB = anchorB;
        AnchorC = anchorC;
        AnchorD = anchorD;
        FlipNormal = flipNormal;
    }
}