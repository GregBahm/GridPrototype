using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BlueprintViewer : MonoBehaviour
{
    public VoxelBlueprint Blueprint;
    public Mesh CurrentMesh;

    public VoxelDesignationDisplay X0Y0Z0Display;
    public VoxelDesignationDisplay X0Y0Z1Display;
    public VoxelDesignationDisplay X0Y1Z0Display;
    public VoxelDesignationDisplay X0Y1Z1Display;
    public VoxelDesignationDisplay X1Y0Z0Display;
    public VoxelDesignationDisplay X1Y0Z1Display;
    public VoxelDesignationDisplay X1Y1Z0Display;
    public VoxelDesignationDisplay X1Y1Z1Display;

    public string GeneratedName;
    public bool MakeStubFromBlueprint;
    public bool SetCorrectBlueprintName;
    public bool FindArtContentForBlueprint;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    public void CorrectCurrentBlueprintName()
    {
        string originalPath = AssetDatabase.GetAssetPath(Blueprint);
        string errorMessageIfAny = AssetDatabase.RenameAsset(originalPath, GeneratedName);
    }

    private void Update()
    {
        CurrentMesh = Blueprint.ArtContent;
        GeneratedName = Blueprint.GetCorrectAssetName();

        MeshFilter.mesh = Blueprint.ArtContent;
        if(Blueprint.Materials != null)
        {
            string[] matNames = Blueprint.Materials.Select(item => item.name).ToArray();
            MeshRenderer.materials = GetViewerMaterials(matNames).ToArray();
        }
        SetDesignationDisplay();
        name = GeneratedName + (Blueprint.ArtContent == null ? (Blueprint.ArtContentless ? " (Contentless)" : "  (Missing Art)") : "");
        HandleCommands();
    }

    private IEnumerable<Material> GetViewerMaterials(string[] materialNames)
    {
        foreach (string name in materialNames)
        {
            switch (name)
            {
                case "PlatformMat":
                    yield return VoxelVisualBaseAssets.Instance.PlatformMat;
                    break;
                case "SlantedRoofMat":
                    yield return VoxelVisualBaseAssets.Instance.SlantedRoofMat;
                    break;
                case "WallMat":
                    yield return VoxelVisualBaseAssets.Instance.WallMat;
                    break;
                case "StrutMat":
                    yield return VoxelVisualBaseAssets.Instance.StrutMat;
                    break;
                default:
                    throw new Exception("Never heard of " + name + " material");
            }
        }
    }

    private IEnumerable<Material> FindMaterials()
    {
        string assetPath = VoxelBlueprint.BlueprintsFolderPath + GeneratedName + ".fbx";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        string[] materialNames = prefab.GetComponent<MeshRenderer>().sharedMaterials.Select(item => item.name).ToArray();
        return GetViewerMaterials(materialNames);
    }

    private void HandleCommands()
    {
        GeneratedName = VoxelBlueprint.GetCorrectAssetName(Blueprint);
        if (SetCorrectBlueprintName)
        {
            SetCorrectBlueprintName = false;
            CorrectCurrentBlueprintName();
        }
        if (MakeStubFromBlueprint)
        {
            MakeStubFromBlueprint = false;
            StubBlueprintFromCurrent();
        }
        if (FindArtContentForBlueprint)
        {
            FindArtContentForBlueprint = false;
            FindArtContent();
        }
    }

    private void FindArtContent()
    {
        string[] assets = AssetDatabase.FindAssets(GeneratedName + " t:Mesh", new[] { VoxelBlueprint.BlueprintsFolderPath });
        if (assets.Length == 1)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(assets[0]));
            Blueprint.ArtContent = mesh;
            name = GeneratedName + Blueprint.ArtContent == null ? " (Empty)" : "";
            Blueprint.Materials = FindMaterials().ToArray();
            EditorUtility.SetDirty(Blueprint);
        }
    }

    public void StubBlueprintFromCurrent()
    {
        VoxelBlueprint blueprint = ScriptableObject.CreateInstance<VoxelBlueprint>();

        blueprint.Up = Blueprint.Up;
        blueprint.Down = Blueprint.Down;

        blueprint.Designations = new DesignationGrid();
        blueprint.Designations.X0Y0Z0 = Blueprint.Designations.X0Y0Z0;
        blueprint.Designations.X0Y0Z1 = Blueprint.Designations.X0Y0Z1;
        blueprint.Designations.X0Y1Z0 = Blueprint.Designations.X0Y1Z0;
        blueprint.Designations.X0Y1Z1 = Blueprint.Designations.X0Y1Z1;
        blueprint.Designations.X1Y0Z0 = Blueprint.Designations.X1Y0Z0;
        blueprint.Designations.X1Y0Z1 = Blueprint.Designations.X1Y0Z1;
        blueprint.Designations.X1Y1Z0 = Blueprint.Designations.X1Y1Z0;
        blueprint.Designations.X1Y1Z1 = Blueprint.Designations.X1Y1Z1;
        blueprint.ArtContent = Blueprint.ArtContent;
        blueprint.name = blueprint.GetCorrectAssetName();
        string path = GetCorrectAssetPath();
        AssetDatabase.CreateAsset(blueprint, path);
    }

    public string GetCorrectAssetPath()
    {
        return VoxelBlueprint.BlueprintsFolderPath + Blueprint.GetCorrectAssetName() + ".asset";
    }

    private void SetDesignationDisplay()
    {
        X0Y0Z0Display.UpdateDisplayContent(Blueprint.Designations.X0Y0Z0);
        X0Y0Z1Display.UpdateDisplayContent(Blueprint.Designations.X0Y0Z1);
        X0Y1Z0Display.UpdateDisplayContent(Blueprint.Designations.X0Y1Z0);
        X0Y1Z1Display.UpdateDisplayContent(Blueprint.Designations.X0Y1Z1);
        X1Y0Z0Display.UpdateDisplayContent(Blueprint.Designations.X1Y0Z0);
        X1Y0Z1Display.UpdateDisplayContent(Blueprint.Designations.X1Y0Z1);
        X1Y1Z0Display.UpdateDisplayContent(Blueprint.Designations.X1Y1Z0);
        X1Y1Z1Display.UpdateDisplayContent(Blueprint.Designations.X1Y1Z1);
    }
}
