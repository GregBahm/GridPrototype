using System;
using UnityEngine;

[Serializable]
public class ComponentInSet
{
    [SerializeField]
    private VoxelVisualComponent component;
    public VoxelVisualComponent Component => component;
    [SerializeField]
    private bool flipped;
    public bool Flipped => flipped;
    [SerializeField]
    private int rotations;
    public int Rotations => rotations;

    public ComponentInSet(VoxelVisualComponent component, 
        bool flipped, 
        int rotations)
    {
        this.component = component;
        this.flipped = flipped;
        this.rotations = rotations;
    }
}
