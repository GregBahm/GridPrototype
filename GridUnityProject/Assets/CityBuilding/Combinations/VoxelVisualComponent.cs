using System;
using UnityEngine;

[CreateAssetMenu(menuName = "VoxelVisualComponent")]
public class VoxelVisualComponent : ScriptableObject
{
    [SerializeField]
    private Mesh mesh;
    [SerializeField]
    private Material[] materials;

    public Mesh Mesh { get => mesh; set => mesh = value; }
    public Material[] Materials { get => materials; set => materials = value; }
}