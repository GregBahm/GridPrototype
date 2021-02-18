using System;
using System.Collections;
using System.Collections.Generic;
using VisualsSolver;

public class NeighborComponents : IEnumerable<CellConnection>
{
    private IEnumerable<CellConnection> cellConnections;

    public CellConnection Up { get; }
    public CellConnection Down { get; }
    public CellConnection Forward { get; }
    public CellConnection Backward { get; }
    public CellConnection Left { get; }
    public CellConnection Right { get; }

    public NeighborComponents(
        VoxelVisualComponent up,
        VoxelVisualComponent down,
        VoxelVisualComponent forward,
        VoxelVisualComponent backward,
        VoxelVisualComponent left,
        VoxelVisualComponent right)
    {
        Up = new CellConnection(up, ConnectsUp);
        Down = new CellConnection(down, ConnectsDown);
        Forward = new CellConnection(forward, ConnectsForward);
        Backward = new CellConnection(backward, ConnectsBack);
        Left = new CellConnection(left, ConnectsLeft);
        Right = new CellConnection(right, ConnectsRight);

        cellConnections = new CellConnection[]
        {
            Up,
            Down,
            Forward,
            Backward,
            Left,
            Right
        };
    }

    private static bool ConnectsUp(VoxelVisualOption source, VoxelVisualOption neighbor)
    {
        return source.Connections.Up == neighbor.Connections.Down;
    }

    private static bool ConnectsDown(VoxelVisualOption source, VoxelVisualOption neighbor)
    {
        return source.Connections.Down == neighbor.Connections.Up;
    }
    private static bool ConnectsLeft(VoxelVisualOption source, VoxelVisualOption neighbor)
    {
        return source.Connections.Left == neighbor.Connections.Right;
    }
    private static bool ConnectsRight(VoxelVisualOption source, VoxelVisualOption neighbor)
    {
        return source.Connections.Right == neighbor.Connections.Left;
    }
    private static bool ConnectsForward(VoxelVisualOption source, VoxelVisualOption neighbor)
    {
        return source.Connections.Forward == neighbor.Connections.Back;
    }
    private static bool ConnectsBack(VoxelVisualOption source, VoxelVisualOption neighbor)
    {
        return source.Connections.Back == neighbor.Connections.Forward;
    }

    public IEnumerator<CellConnection> GetEnumerator()
    {
        return cellConnections.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}