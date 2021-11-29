using UnityEngine;

public class VoxelVisualOption
{
    public Mesh Mesh { get; }
    private readonly bool[,,] designation;

    public int Priority { get; }

    public bool Flipped { get; }
    public int Rotations { get; }

    public VoxelVisualConnections Connections { get; }

    public VoxelVisualOption(Mesh mesh, bool[,,] designation, bool flipped, int rotations, int priority, VoxelVisualConnections connections)
    {
        Mesh = mesh;
        this.designation = designation;
        Flipped = flipped;
        Rotations = rotations;
        Priority = priority;
        Connections = connections;
    }

    public string GetDesignationKey()
    {
        string ret = "";
        foreach (bool item in designation)
        {
            ret += item.ToString() + " ";
        }
        return ret;
    }

    public override string ToString()
    {
        return Mesh?.name;
    }
}
