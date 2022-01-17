using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "VoxelDefinition/VoxelBlueprint")]
public class VoxelBlueprint : ScriptableObject
{
    public const string BlueprintsFolderPath = "Assets/CityBuilding/VoxelBlueprints/";

    public Mesh ArtContent;
    public Material[] Materials;

    public VoxelConnectionType Up;
    public VoxelConnectionType Down;
    public DesignationGrid Designations;

    public IEnumerable<VisualCellOption> GenerateVisualOptions()
    {
        VisualCellConnections baseConnections = new VisualCellConnections(Up, Down);

        VoxelDesignation baseDesignation = new VoxelDesignation(Designations.ToFlatArray());
        int priority = 0;
        yield return new VisualCellOption(ArtContent, Materials, baseDesignation.Description, false, 0, priority, baseConnections);
        IEnumerable<GeneratedVoxelDesignation> variants = baseDesignation.GetUniqueVariants().ToArray();
        foreach (GeneratedVoxelDesignation variant in variants)
        {
            priority++;
            yield return new VisualCellOption(ArtContent, Materials, variant.Description, variant.WasFlipped, variant.Rotations, priority, baseConnections);
        }
    }

    public static VoxelBlueprint[] GetAllBlueprints()
    {
        string[] guids = AssetDatabase.FindAssets("t: VoxelBlueprint", new[] { VoxelBlueprint.BlueprintsFolderPath });
        List<VoxelBlueprint> ret = guids.Select(item => AssetDatabase.LoadAssetAtPath<VoxelBlueprint>(AssetDatabase.GUIDToAssetPath(item))).ToList();
        ret.Reverse();
        return ret.ToArray();
    }

    public string GetCorrectAssetName()
    {
        return GetCorrectAssetName(this);
    }

    public static string GetCorrectAssetName(VoxelBlueprint blueprint)
    {
        return blueprint.Down.ToString() + "_" +
            GetNameComponent(blueprint.Designations.X0Y0Z0) + "_" +
            GetNameComponent(blueprint.Designations.X0Y0Z1) + "_" +
            GetNameComponent(blueprint.Designations.X0Y1Z0) + "_" +
            GetNameComponent(blueprint.Designations.X0Y1Z1) + "_" +
            GetNameComponent(blueprint.Designations.X1Y0Z0) + "_" +
            GetNameComponent(blueprint.Designations.X1Y0Z1) + "_" +
            GetNameComponent(blueprint.Designations.X1Y1Z0) + "_" +
            GetNameComponent(blueprint.Designations.X1Y1Z1) + "_" +
                blueprint.Up.ToString();
    }

    private static string GetNameComponent(VoxelDesignationType slotType)
    {
        switch (slotType)
        {
            case VoxelDesignationType.Empty:
                return "E";
            case VoxelDesignationType.AnyFilled:
                return "A";
            case VoxelDesignationType.SlantedRoof:
                return "S";
            case VoxelDesignationType.WalkableRoof:
                return "W";
            case VoxelDesignationType.Platform:
                return "P";
            case VoxelDesignationType.Ground:
            default:
                return "G";
        }
    }

    public static VoxelDesignationType GetSlotFromName(string firstLetter)
    {
        switch (firstLetter)
        {
            case "E":
                return VoxelDesignationType.Empty;
            case "A":
                return VoxelDesignationType.AnyFilled;
            case "S":
                return VoxelDesignationType.SlantedRoof;
            case "W":
                return VoxelDesignationType.WalkableRoof;
            case "P":
                return VoxelDesignationType.Platform;
            case "G":
                return VoxelDesignationType.Ground;
            default:
                throw new Exception("No slot startting with letter \"" + firstLetter + "\"");
        }
    }

    public static string GetBlueprintAssetPath(VoxelBlueprint blueprint)
    {
        string name = GetCorrectAssetName(blueprint);
        return BlueprintsFolderPath + name + ".asset";
    }
}

[Serializable]
public class DesignationGrid
{
    public VoxelDesignationType X0Y0Z0;
    public VoxelDesignationType X0Y0Z1;
    public VoxelDesignationType X0Y1Z0;
    public VoxelDesignationType X0Y1Z1;
    public VoxelDesignationType X1Y0Z0;
    public VoxelDesignationType X1Y0Z1;
    public VoxelDesignationType X1Y1Z0;
    public VoxelDesignationType X1Y1Z1;

    public VoxelDesignationType[] ToFlatArray()
    {
        return new VoxelDesignationType[]
        {
            X0Y0Z0,
            X0Y0Z1,
            X0Y1Z0,
            X0Y1Z1,
            X1Y0Z0,
            X1Y0Z1,
            X1Y1Z0,
            X1Y1Z1,
        };
    }

    public static DesignationGrid FromDesignation(VoxelDesignation designation)
    {
        DesignationGrid ret = new DesignationGrid();
        ret.X0Y0Z0 = designation.Description[0, 0, 0];
        ret.X0Y0Z1 = designation.Description[0, 0, 1];
        ret.X0Y1Z0 = designation.Description[0, 1, 0];
        ret.X0Y1Z1 = designation.Description[0, 1, 1];
        ret.X1Y0Z0 = designation.Description[1, 0, 0];
        ret.X1Y0Z1 = designation.Description[1, 0, 1];
        ret.X1Y1Z0 = designation.Description[1, 1, 0];
        ret.X1Y1Z1 = designation.Description[1, 1, 1];
        return ret;
    }

    internal static DesignationGrid FromCubeArray(VoxelDesignationType[,,] values)
    {
        DesignationGrid ret = new DesignationGrid();
        ret.X0Y0Z0 = values[0, 0, 0];
        ret.X0Y0Z1 = values[0, 0, 1];
        ret.X0Y1Z0 = values[0, 1, 0];
        ret.X0Y1Z1 = values[0, 1, 1];
        ret.X1Y0Z0 = values[1, 0, 0];
        ret.X1Y0Z1 = values[1, 0, 1];
        ret.X1Y1Z0 = values[1, 1, 0];
        ret.X1Y1Z1 = values[1, 1, 1];
        return ret;
    }

    public static DesignationGrid FromFlatArray(VoxelDesignationType[] values)
    {
        DesignationGrid ret = new DesignationGrid();
        ret.X0Y0Z0 = values[0];
        ret.X0Y0Z1 = values[1];
        ret.X0Y1Z0 = values[2];
        ret.X0Y1Z1 = values[3];
        ret.X1Y0Z0 = values[4];
        ret.X1Y0Z1 = values[5];
        ret.X1Y1Z0 = values[6];
        ret.X1Y1Z1 = values[7];
        return ret;
    }

    public VoxelDesignationType[,,] ToCubedArray()
    {
        VoxelDesignationType[,,] ret = new VoxelDesignationType[2, 2, 2];
        ret[0, 0, 0] = X0Y0Z0;
        ret[0, 0, 1] = X0Y0Z1;
        ret[0, 1, 0] = X0Y1Z0;
        ret[0, 1, 1] = X0Y1Z1;
        ret[1, 0, 0] = X1Y0Z0;
        ret[1, 0, 1] = X1Y0Z1;
        ret[1, 1, 0] = X1Y1Z0;
        ret[1, 1, 1] = X1Y1Z1;
        return ret;
    }
}
