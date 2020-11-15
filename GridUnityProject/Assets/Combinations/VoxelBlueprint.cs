using UnityEngine;

[CreateAssetMenu(menuName = "VoxelDefinition/VoxelBlueprint")]
public class VoxelBlueprint : ScriptableObject
{
    public GameObject ArtContent;
    public bool[] DesignationValues;
    public VoxelConnection Up;
    public VoxelConnection Down;
    public VoxelConnection PositiveX;
    public VoxelConnection NegativeX;
    public VoxelConnection PositiveZ;
    public VoxelConnection NegativeZ;
}
