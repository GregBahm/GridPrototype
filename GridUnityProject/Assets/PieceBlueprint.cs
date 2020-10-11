using GameGrid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualPieceProfile : MonoBehaviour
{
    public VisualSet CoreSet { get; }
    public VisualSet ConectionUpward { get; }
    public VisualSet ConnectionDownward { get; }

    public VisualSet ConnectionX { get; }
    public VisualSet ConnectionZ { get; }
}

public class VoxelVisuals
{
    public VoxelCell Voxel { get; }
    private readonly Dictionary<GroundEdge, PieceSlot> lowerPieces = new Dictionary<GroundEdge, PieceSlot>();
    private readonly Dictionary<GroundEdge, PieceSlot> upperPieces = new Dictionary<GroundEdge, PieceSlot>();

    public VoxelVisuals(VoxelCell cell)
    {
        foreach (GroundEdge edge in Voxel.GroundPoint.Edges)
        {
            upperPieces.Add(edge, new PieceSlot(this, true, edge));
            lowerPieces.Add(edge, new PieceSlot(this, false, edge));
        }
    }
}

public class PieceSlot
{
    public VoxelVisuals Home { get; }
    public bool OnUpperHalf { get; }
    public GroundEdge Edge { get; }

    public PieceSlot(VoxelVisuals home, bool onUpperHalf, GroundEdge edge)
    {
        Home = home;
        OnUpperHalf = onUpperHalf;
        Edge = edge;
    }
}

public enum VisualSet
{
    Ground,
    Air,
    Structure,
    Walkway
}