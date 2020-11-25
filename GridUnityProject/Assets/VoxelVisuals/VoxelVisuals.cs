using System;
using System.Collections.Generic;
using System.Linq;
using GameGrid;

public class VoxelVisuals
{
    public VoxelCell Cell { get; }

    public IEnumerable<VoxelVisualComponent> Components { get; }

    public VoxelVisuals(VoxelCell cell)
    {
        Cell = cell;
        Components = CreateVisualComponents().ToArray();
    }

    private IEnumerable<VoxelVisualComponent> CreateVisualComponents()
    {
        foreach (GroundQuad quad in Cell.GroundPoint.PolyConnections)
        {
            yield return new VoxelVisualComponent(Cell, quad, false);
            yield return new VoxelVisualComponent(Cell, quad, true);
        }
    }
}
