using UnityEngine;

public class VoxelVisualOption
{
    public Mesh Mesh { get; }
    private readonly bool[,,] designation;

    public int Priority { get; }

    public bool Flipped { get; }
    public int Rotations { get; }

    public VoxelVisualConnections Connections { get; }

    public bool IsGround { get; }

    public VoxelVisualOption(Mesh mesh, bool[,,] designation, bool flipped, int rotations, int priority, VoxelVisualConnections connections, bool isGround)
    {
        Mesh = mesh;
        this.designation = designation;
        Flipped = flipped;
        Rotations = rotations;
        Priority = priority;
        Connections = connections;
        IsGround = isGround;
    }

    public string GetDesignationKey()
    {
        string ret = "";
        foreach (bool item in designation)
        {
            ret += item.ToString() + " ";
        }
        ret += IsGround ? " Ground" : "";
        return ret;
    }
}
