using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameGrid;

public class VoxelVisuals
{
    public VoxelCell Cell { get; }

    private readonly IReadOnlyDictionary<GroundQuad, VoxelVisualComponent> bottomComponents;
    private readonly IReadOnlyDictionary<GroundQuad, VoxelVisualComponent> topComponents;
    
    public IEnumerable<VoxelVisualComponent> Components { get; }

    public VoxelVisuals(VoxelCell cell)
    {
        Cell = cell;
        Dictionary<GroundQuad, VoxelVisualComponent> bottoms = new Dictionary<GroundQuad, VoxelVisualComponent>();
        Dictionary<GroundQuad, VoxelVisualComponent> tops = new Dictionary<GroundQuad, VoxelVisualComponent>();
        foreach (GroundQuad quad in cell.GroundPoint.PolyConnections)
        {
            VoxelVisualComponent bottom = new VoxelVisualComponent(Cell, quad, false);
            VoxelVisualComponent top = new VoxelVisualComponent(Cell, quad, true);
            bottoms.Add(quad, bottom);
            tops.Add(quad, top);
        }
        bottomComponents = bottoms;
        topComponents = tops;
        Components = bottomComponents.Values.Concat(topComponents.Values).ToArray();
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
