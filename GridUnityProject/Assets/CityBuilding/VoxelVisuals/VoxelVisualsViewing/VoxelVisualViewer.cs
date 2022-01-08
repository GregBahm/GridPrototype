using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class VoxelVisualViewer : MonoBehaviour
{
    public static VoxelVisualViewer Instance { get; private set; }
    public GameObject BlueprintViewerPrefab;

    private BlueprintViewer[] blueprintViewers;
    public VoxelVisualColors Colors;

    public float Margin;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        List<VoxelBlueprint> allBlueprints = VoxelBlueprint.GetAllBlueprints().ToList();
        blueprintViewers = InstantiateBlueprints(allBlueprints).ToArray();
    }

    private void Update()
    {
        ArrangeBlueprints();
    }

    private IEnumerable<BlueprintViewer> InstantiateBlueprints(List<VoxelBlueprint> allBlueprints)
    {
        foreach (VoxelBlueprint blueprint in allBlueprints)
        {
            yield return InstantiateBlueprint(blueprint);
        }
    }

    private BlueprintViewer InstantiateBlueprint(VoxelBlueprint blueprint)
    {
        GameObject gameObj = Instantiate(BlueprintViewerPrefab);
        BlueprintViewer ret = gameObj.GetComponent<BlueprintViewer>();
        ret.Blueprint = blueprint;
        return ret;
    }

    private void ArrangeBlueprints()
    {
        List<List<BlueprintViewer>> items = GetSortedBlueprints();
        for (int yIndex = 0; yIndex < items.Count; yIndex++)
        {
            List<BlueprintViewer> row = items[yIndex];
            for (int xIndex = 0; xIndex < row.Count; xIndex++)
            {
                BlueprintViewer blueprint = row[xIndex];
                PlaceBlueprint(blueprint.transform, xIndex, yIndex);
            }
        }
    }

    private void PlaceBlueprint(Transform transform, int xIndex, int yIndex)
    {
        transform.position = new Vector3(-xIndex * (1f + Margin), 0, -yIndex * (1f + Margin));
    }

    private List<List<BlueprintViewer>> GetSortedBlueprints()
    {
        List<BlueprintViewer> piecesWithOnlySlanted = new List<BlueprintViewer>();
        List<BlueprintViewer> piecesWithOnlyWalkable = new List<BlueprintViewer>();
        List<BlueprintViewer> piecesWithBoth = new List<BlueprintViewer>();
        List<BlueprintViewer> piecesWithNeather = new List<BlueprintViewer>();

        foreach (BlueprintViewer blueprint in blueprintViewers)
        {
            VoxelDesignationType[] designations = blueprint.Blueprint.Designations.ToFlatArray();
            bool hasSlanted = designations.Any(item => item == VoxelDesignationType.SlantedRoof);
            bool hasWalkable = designations.Any(item => item == VoxelDesignationType.WalkableRoof);
            if (hasSlanted && hasWalkable)
                piecesWithBoth.Add(blueprint);
            else if (hasSlanted)
                piecesWithOnlySlanted.Add(blueprint);
            else if (hasWalkable)
                piecesWithOnlyWalkable.Add(blueprint);
            else
                piecesWithNeather.Add(blueprint);
        }

        return new List<List<BlueprintViewer>> { piecesWithOnlySlanted, piecesWithOnlyWalkable, piecesWithBoth, piecesWithNeather };
    }
}

[Serializable]
public class VoxelVisualColors
{
    public Color AnyFilled;
    public Color WalkableRoof;
    public Color SlantedRoof;
    public Color Platform;
    public Color Ground;

    public Color GetColorFor(VoxelDesignationType slotType)
    {
        switch (slotType)
        {
            case VoxelDesignationType.AnyFilled:
                return AnyFilled;
            case VoxelDesignationType.SlantedRoof:
                return SlantedRoof;
            case VoxelDesignationType.WalkableRoof:
                return WalkableRoof;
            case VoxelDesignationType.Platform:
                return Platform;
            case VoxelDesignationType.Empty:
            case VoxelDesignationType.Ground:
            default:
                return Ground;
        }
    }
}