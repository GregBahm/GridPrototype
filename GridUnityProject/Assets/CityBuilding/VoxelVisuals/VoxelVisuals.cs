using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameGrid;
using UnityEngine;

public class VoxelVisuals
{
    public VoxelCell Cell { get; }

    private readonly VoxelVisualComponent[] topComponentsArray;
    private readonly VoxelVisualComponent[] bottomComponentsArray;
    private readonly IReadOnlyDictionary<GroundQuad, VoxelVisualComponent> bottomComponents;
    private readonly IReadOnlyDictionary<GroundQuad, VoxelVisualComponent> topComponents;
    
    public IEnumerable<VoxelVisualComponent> Components { get; }

    public VoxelVisuals(VoxelCell cell)
    {
        Cell = cell;
        Dictionary<GroundQuad, VoxelVisualComponent> bottoms = new Dictionary<GroundQuad, VoxelVisualComponent>();
        Dictionary<GroundQuad, VoxelVisualComponent> tops = new Dictionary<GroundQuad, VoxelVisualComponent>();

        GroundQuad[] quads = cell.GroundPoint.PolyConnections.OrderByDescending(item => GetSignedAngle(item, cell.GroundPoint)).ToArray();

        topComponentsArray = new VoxelVisualComponent[quads.Length];
        bottomComponentsArray = new VoxelVisualComponent[quads.Length];
        for (int i = 0; i < quads.Length; i++)
        {
            GroundQuad quad = quads[i];
            VoxelVisualComponent bottom = new VoxelVisualComponent(Cell, quad, false, i);
            VoxelVisualComponent top = new VoxelVisualComponent(Cell, quad, true, i);
            bottoms.Add(quad, bottom);
            tops.Add(quad, top);
            topComponentsArray[i] = top;
            bottomComponentsArray[i] = bottom;
        }
        bottomComponents = bottoms;
        topComponents = tops;
        Components = bottomComponents.Values.Concat(topComponents.Values).ToArray();
    }

    private float GetSignedAngle(GroundQuad quad, GroundPoint point)
    {
        return Vector2.SignedAngle(Vector2.up, quad.Center - point.Position);
    }

    public VoxelVisualComponent GetLeftNeighbor(VoxelVisualComponent component)
    {
        VoxelVisualComponent[] set = component.OnTopHalf ? topComponentsArray : bottomComponentsArray;
        int neighborIndex = (component.VisualsIndex + 1) % set.Length;
        return set[neighborIndex];
    }
    public VoxelVisualComponent GetForwardNeighbor(VoxelVisualComponent component)
    {
        VoxelVisualComponent[] set = component.OnTopHalf ? topComponentsArray : bottomComponentsArray;
        int neighborIndex = (component.VisualsIndex - 1 + set.Length) % set.Length; ;
        return set[neighborIndex];
    }

    public VoxelVisualComponent GetComponent(GroundQuad quad, bool onTopHalf)
    {
        if(onTopHalf)
        {
            return topComponents[quad];
        }
        return bottomComponents[quad];
    }
}
