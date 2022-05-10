using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class EditorTools : MonoBehaviour
{

    [MenuItem("Blueprints/Populate Blueprints")]
    static void PopulateBlueprints()
    {
        VoxelBlueprint[] blueprints = GetAllBlueprints();
        GameObject obj = Selection.activeGameObject;
        obj.GetComponent<CityBuildingMain>().Blueprints = blueprints;
    }

    public static VoxelBlueprint[] GetAllBlueprints()
    {
        string[] guids = AssetDatabase.FindAssets("t: VoxelBlueprint", new[] { VoxelBlueprint.BlueprintsFolderPath });
        List<VoxelBlueprint> ret = guids.Select(item => AssetDatabase.LoadAssetAtPath<VoxelBlueprint>(AssetDatabase.GUIDToAssetPath(item))).ToList();
        //ret.Reverse();
        return ret.ToArray();
    }
}