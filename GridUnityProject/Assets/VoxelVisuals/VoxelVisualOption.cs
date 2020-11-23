using UnityEngine;

public class VoxelVisualOption
{
    public Mesh Mesh { get; }
    private readonly bool[,,] designation;

    public bool Flipped { get; }
    public int Rotations { get; }

    public VoxelVisualOption(Mesh mesh, bool[,,] designation, bool flipped, int rotations)
    {
        Mesh = mesh;
        this.designation = designation;
        Flipped = flipped;
        Rotations = rotations;
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

    // TODO: Connections


}