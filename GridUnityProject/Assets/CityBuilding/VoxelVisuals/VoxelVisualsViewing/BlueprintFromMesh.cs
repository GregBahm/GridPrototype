using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BlueprintFromMesh : MonoBehaviour
{
    [MenuItem("Blueprints/Create Blueprint From Mesh")]
    static void CreateBlueprintFromMesh()
    {
        Mesh mesh = Selection.GetFiltered<Mesh>(SelectionMode.Unfiltered)[0];
        string[] meshComponents = mesh.name.Split('_');

        VoxelConnectionType downConnection = GetConnectionType(meshComponents[0]);
        VoxelDesignationType x0y0z0 = GetSlotType(meshComponents[1]);
        VoxelDesignationType x0y0z1 = GetSlotType(meshComponents[2]);
        VoxelDesignationType x0y1z0 = GetSlotType(meshComponents[3]);
        VoxelDesignationType x0y1z1 = GetSlotType(meshComponents[4]);
        VoxelDesignationType x1y0z0 = GetSlotType(meshComponents[5]);
        VoxelDesignationType x1y0z1 = GetSlotType(meshComponents[6]);
        VoxelDesignationType x1y1z0 = GetSlotType(meshComponents[7]);
        VoxelDesignationType x1y1z1 = GetSlotType(meshComponents[8]);
        VoxelConnectionType upConnection = GetConnectionType(meshComponents[9]);

        VoxelBlueprint blueprint = ScriptableObject.CreateInstance<VoxelBlueprint>();
        blueprint.ArtContent = mesh;
        blueprint.Up = upConnection;
        blueprint.Down = downConnection;
        blueprint.Designations = new DesignationGrid();
        blueprint.Designations.X0Y0Z0 = x0y0z0;
        blueprint.Designations.X0Y0Z1 = x0y0z1;
        blueprint.Designations.X0Y1Z0 = x0y1z0;
        blueprint.Designations.X0Y1Z1 = x0y1z1;
        blueprint.Designations.X1Y0Z0 = x1y0z0;
        blueprint.Designations.X1Y0Z1 = x1y0z1;
        blueprint.Designations.X1Y1Z0 = x1y1z0;
        blueprint.Designations.X1Y1Z0 = x1y1z1;

        string path = VoxelBlueprint.GetBlueprintAssetPath(blueprint);
        AssetDatabase.CreateAsset(blueprint, path);
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = blueprint;
    }

    private static VoxelDesignationType GetSlotType(string slotLetter)
    {
        return VoxelBlueprint.GetSlotFromName(slotLetter);
    }

    private static VoxelConnectionType GetConnectionType(string connectionAsString)
    {
        return (VoxelConnectionType)Enum.Parse(typeof(VoxelConnectionType), connectionAsString);
    }

    [MenuItem("Blueprints/Create Blueprint From Mesh", true)]
    static bool ValidateCreateBlueprintFromMesh()
    {
        Mesh[] meshes = Selection.GetFiltered<Mesh>(SelectionMode.Unfiltered);
        if(meshes.Length == 1)
        {
            Mesh mesh = meshes[0];
            string[] components = mesh.name.Split('_');
            return components.Length == 10;
        }
        return false;
    }
}
