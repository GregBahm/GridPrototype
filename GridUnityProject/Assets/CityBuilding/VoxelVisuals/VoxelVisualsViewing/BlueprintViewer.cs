using System.Collections;
using System.Collections.Generic;
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
    private MeshFilter meshFilter;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void CorrectCurrentBlueprintName()
    {
        string originalPath = AssetDatabase.GetAssetPath(Blueprint);
        string errorMessageIfAny = AssetDatabase.RenameAsset(originalPath, GeneratedName);
    }

    private void Update()
    {
        CurrentMesh = Blueprint.ArtContent;
        GeneratedName = Blueprint.GetCorrectAssetName();

        meshFilter.mesh = Blueprint.ArtContent;
        SetDesignationDisplay();
        name = GeneratedName + (Blueprint.ArtContent == null ? " (Empty)" : "");
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
            EditorUtility.SetDirty(Blueprint);
        }
    }

    public void StubBlueprintFromCurrent()
    {
        VoxelBlueprint blueprint = ScriptableObject.CreateInstance<VoxelBlueprint>();

        blueprint.PositiveX = Blueprint.PositiveX;
        blueprint.Up = Blueprint.Up;
        blueprint.PositiveZ = Blueprint.PositiveZ;
        blueprint.NegativeX = Blueprint.NegativeX;
        blueprint.Down = Blueprint.Down;
        blueprint.NegativeZ = Blueprint.NegativeZ;

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
