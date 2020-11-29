using System;
using System.Collections.Generic;
using System.Linq;
using GameGrid;

public class VoxelVisuals
{
    public VoxelCell Cell { get; }

    private Lazy<IEnumerable<VoxelVisualComponent>> componentsLoader;
    public IEnumerable<VoxelVisualComponent> Components { get { return componentsLoader.Value; } }

    public VoxelVisuals(VoxelCell cell)
    {
        Cell = cell;
        componentsLoader = new Lazy<IEnumerable<VoxelVisualComponent>>(CreateVisualComponents);
    }

    private IEnumerable<VoxelVisualComponent> CreateVisualComponents()
    {
        List<VoxelVisualComponent> ret = new List<VoxelVisualComponent>();
        foreach (GroundQuad quad in Cell.GroundPoint.PolyConnections)
        {
            ret.Add(new VoxelVisualComponent(Cell, quad, false));
            ret.Add(new VoxelVisualComponent(Cell, quad, true));
        }
        return ret;
    }
}
