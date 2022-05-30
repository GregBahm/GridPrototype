using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VoxelVisuals;

public class VoxelBlueprint : ScriptableObject
{
    public const string BlueprintsFolderPath = "Assets/CityBuilding/VoxelBlueprints/";

    public Mesh ArtContent;
    public Material[] Materials;

    public VoxelConnectionType Up;
    public VoxelConnectionType Down;
    public DesignationGrid Designations;

    public bool ArtContentless;

    public IEnumerable<VisualCellOption> GenerateVisualOptions()
    {
        VisualCellConnections baseConnections = new VisualCellConnections(Up, Down);

        VoxelVisualDesignation baseDesignation = new VoxelVisualDesignation(Designations.ToFlatArray());
        int priority = 0;
        yield return new VisualCellOption(ArtContent, Materials, baseDesignation.Description, false, 0, priority, baseConnections);
        IEnumerable<GeneratedVoxelDesignation> variants = baseDesignation.GetUniqueVariants().ToArray();
        foreach (GeneratedVoxelDesignation variant in variants)
        {
            priority++;
            yield return new VisualCellOption(ArtContent, Materials, variant.Description, variant.WasFlipped, variant.Rotations, priority, baseConnections);
        }
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

    private static string GetNameComponent(VoxelDesignation slotType)
    {
        switch (slotType)
        {
            case VoxelDesignation.Empty:
                return "E";
            case VoxelDesignation.AnyFilled:
                return "A";
            case VoxelDesignation.SlantedRoof:
                return "S";
            case VoxelDesignation.WalkableRoof:
                return "W";
            case VoxelDesignation.Platform:
                return "P";
            case VoxelDesignation.Ground:
            default:
                return "G";
        }
    }

    public static VoxelDesignation GetSlotFromName(string firstLetter)
    {
        switch (firstLetter)
        {
            case "E":
                return VoxelDesignation.Empty;
            case "A":
                return VoxelDesignation.AnyFilled;
            case "S":
                return VoxelDesignation.SlantedRoof;
            case "W":
                return VoxelDesignation.WalkableRoof;
            case "P":
                return VoxelDesignation.Platform;
            case "G":
                return VoxelDesignation.Ground;
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
    public VoxelDesignation X0Y0Z0;
    public VoxelDesignation X0Y0Z1;
    public VoxelDesignation X0Y1Z0;
    public VoxelDesignation X0Y1Z1;
    public VoxelDesignation X1Y0Z0;
    public VoxelDesignation X1Y0Z1;
    public VoxelDesignation X1Y1Z0;
    public VoxelDesignation X1Y1Z1;

    public VoxelDesignation[] ToFlatArray()
    {
        return new VoxelDesignation[]
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

    public static DesignationGrid FromDesignation(VoxelVisualDesignation designation)
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

    internal static DesignationGrid FromCubeArray(VoxelDesignation[,,] values)
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

    public static DesignationGrid FromFlatArray(VoxelDesignation[] values)
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

    public VoxelDesignation[,,] ToCubedArray()
    {
        VoxelDesignation[,,] ret = new VoxelDesignation[2, 2, 2];
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
