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
            MeshRenderer.materials = Blueprint.Materials;
        SetDesignationDisplay();
        name = GeneratedName + (Blueprint.ArtContent == null ? (Blueprint.ArtContentless ? " (Contentless)" : "  (Missing Art)") : "");
        HandleCommands();
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
            PopulateMaterials();
            EditorUtility.SetDirty(Blueprint);
        }
    }

    private void PopulateMaterials()
    {
            List<Material> materials = new List<Material>();
            VoxelDesignationType[] designations = Blueprint.Designations.ToFlatArray();
            if (designations.Contains(VoxelDesignationType.AnyFilled)
                || designations.Contains(VoxelDesignationType.SlantedRoof)
                || designations.Contains(VoxelDesignationType.WalkableRoof))
                materials.Add(VoxelVisualBaseAssets.Instance.WallMat);
            if (designations.Contains(VoxelDesignationType.SlantedRoof))
                materials.Add(VoxelVisualBaseAssets.Instance.SlantedRoofMat);
            if (designations.Contains(VoxelDesignationType.WalkableRoof)
                || designations.Contains(VoxelDesignationType.Platform))
                materials.Add(VoxelVisualBaseAssets.Instance.PlatformMat);
            if (designations.Contains(VoxelDesignationType.Ground)
                || Blueprint.Up == VoxelConnectionType.BigStrut
                || Blueprint.Down == VoxelConnectionType.BigStrut)
                materials.Add(VoxelVisualBaseAssets.Instance.StrutMat);
            Blueprint.Materials = materials.ToArray();
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
