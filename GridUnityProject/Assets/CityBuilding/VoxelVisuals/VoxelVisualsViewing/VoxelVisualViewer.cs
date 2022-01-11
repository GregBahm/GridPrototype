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

    public float Margin;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        IEnumerable<VoxelBlueprint> allBlueprints = VoxelBlueprint.GetAllBlueprints().Where(item => item.ArtContent != null);
        OrganizedBlueprints visuals = new OrganizedBlueprints(this, allBlueprints);
        visuals.InstantiateGameObjects();
    }

    private void DoOldPlacementTechnique()
    {
        List<VoxelBlueprint> allBlueprints = VoxelBlueprint.GetAllBlueprints().ToList();
        blueprintViewers = InstantiateBlueprints(allBlueprints).ToArray();
        ArrangeBlueprints();
    }

    private IEnumerable<BlueprintViewer> InstantiateBlueprints(List<VoxelBlueprint> allBlueprints)
    {
        foreach (VoxelBlueprint blueprint in allBlueprints)
        {
            yield return InstantiateBlueprint(blueprint);
        }
    }

    public BlueprintViewer InstantiateBlueprint(VoxelBlueprint blueprint)
    {
        if(blueprint == null)
        {
            Debug.Log("Hey now");
        }
        GameObject gameObj = Instantiate(BlueprintViewerPrefab);
        BlueprintViewer ret = gameObj.GetComponent<BlueprintViewer>();
        ret.GeneratedName = blueprint.GetCorrectAssetName();
        ret.Blueprint = blueprint;
        gameObj.name = blueprint.name;
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

    public void PlaceBlueprint(Transform transform, int xIndex, int yIndex)
    {
        transform.position = new Vector3(-xIndex * (1f + Margin), 0, -yIndex * (1f + Margin));
    }

    private List<List<BlueprintViewer>> GetSortedBlueprints()
    {
        List<BlueprintViewer> piecesWithOnlySlanted = new List<BlueprintViewer>();
        List<BlueprintViewer> piecesWithOnlyWalkable = new List<BlueprintViewer>();
        List<BlueprintViewer> piecesWithBoth = new List<BlueprintViewer>();
        List<BlueprintViewer> piecesWithNeather = new List<BlueprintViewer>();
        List<BlueprintViewer> groundPieces = new List<BlueprintViewer>();

        foreach (BlueprintViewer blueprint in blueprintViewers)
        {
            VoxelDesignationType[] designations = blueprint.Blueprint.Designations.ToFlatArray();
            bool hasSlanted = designations.Any(item => item == VoxelDesignationType.SlantedRoof);
            bool hasWalkable = designations.Any(item => item == VoxelDesignationType.WalkableRoof);
            bool hasGround = designations.Any(item => item == VoxelDesignationType.Ground);
            if (hasSlanted && hasWalkable)
                piecesWithBoth.Add(blueprint);
            else if (hasSlanted)
                piecesWithOnlySlanted.Add(blueprint);
            else if (hasWalkable)
                piecesWithOnlyWalkable.Add(blueprint);
            else if (hasGround)
                groundPieces.Add(blueprint);
            else
                piecesWithNeather.Add(blueprint);
        }

        return new List<List<BlueprintViewer>> { piecesWithOnlySlanted, piecesWithOnlyWalkable, piecesWithBoth, groundPieces, piecesWithNeather };
    }

    public static string GetInvariantKey(VoxelBlueprint blueprint)
    {
        IEnumerable<VisualCellOption> options = blueprint.GenerateVisualOptions();
        List<string> asKeys = options.Select(item => GetVisualCellOptionKey(item)).ToList();
        asKeys.Sort();
        return asKeys[0];
    }

    private static string GetVisualCellOptionKey(VisualCellOption option)
    {
        return option.Connections.Down.ToString() + "_" + option.GetDesignationKey() + "_" + option.Connections.Up.ToString();
    }

    private class OrganizedBlueprints
    {
        private readonly VoxelVisualViewer mothership;
        private readonly List<PotentialStrutPair> nonRoofPieces;
        private readonly List<RoofPieceGroup> roofPieces;
        private readonly IEnumerable<VoxelBlueprint> allBlueprints;
        private readonly Dictionary<string, VoxelBlueprint> pieceDictionary;

        public OrganizedBlueprints(VoxelVisualViewer mothership, IEnumerable<VoxelBlueprint> allBlueprints)
        {
            this.mothership = mothership;
            this.allBlueprints = allBlueprints;
            pieceDictionary = new Dictionary<string, VoxelBlueprint>();
            this.pieceDictionary = allBlueprints.ToDictionary(item => GetInvariantKey(item), item => item);

            roofPieces = GetRoofPieceGroups().ToList();
            nonRoofPieces = GetNonRoofPieces().ToList();
        }

        private IEnumerable<PotentialStrutPair> GetNonRoofPieces()
        {
            foreach (VoxelBlueprint blueprint in allBlueprints.Where(item => item.Up != VoxelConnectionType.BigStrut))
            {
                VoxelDesignationType[] designations = blueprint.Designations.ToFlatArray();
                if (designations.All(item => item != VoxelDesignationType.SlantedRoof 
                        && item != VoxelDesignationType.WalkableRoof))
                {
                    BlueprintContainer container = new BlueprintContainer(blueprint, blueprint);
                    PotentialStrutPair pair = new PotentialStrutPair(container, pieceDictionary);
                    yield return pair;
                }
            }
        }

        internal void InstantiateGameObjects()
        {
            InstantiateNonRoofPieces();
            InstantiateRoofPieces();
        }

        private void InstantiateRoofPieces()
        {
            for (int i = 0; i < roofPieces.Count; i++)
            {
                RoofPieceGroup group = roofPieces[i];
                CreateAndPlacePiecePair(group.SlantedPiece, i, 2);
                CreateAndPlacePiecePair(group.WalkablePiece, i, 4);
            }
        }
        private IEnumerable<RoofPieceGroup> GetRoofPieceGroups()
        {
            IEnumerable<VoxelBlueprint> slantPieces = GetBaseSlantPieces(allBlueprints);
            List<RoofPieceGroup> ret = new List<RoofPieceGroup>();
            foreach (VoxelBlueprint slantPiece in slantPieces)
            {
                RoofPieceGroup group = new RoofPieceGroup(slantPiece, pieceDictionary);
                ret.Add(group);
            }
            return ret;
        }
        private IEnumerable<VoxelBlueprint> GetBaseSlantPieces(IEnumerable<VoxelBlueprint> allBlueprints)
        {
            foreach (VoxelBlueprint blueprint in allBlueprints.Where(item => item.Up != VoxelConnectionType.BigStrut))
            {
                IEnumerable<VoxelDesignationType> slots = blueprint.Designations.ToFlatArray();
                if (slots.Any(item => item == VoxelDesignationType.SlantedRoof) &&
                    !slots.Any(item => item == VoxelDesignationType.WalkableRoof))
                    yield return blueprint;
            }
        }

        private void CreateAndPlacePiecePair(PotentialStrutPair pair, int xOffset, int yOffset)
        {
            VoxelBlueprint blueprint = pair.BasePiece.BestBlueprint;
            BlueprintViewer viewer = mothership.InstantiateBlueprint(blueprint);
            mothership.PlaceBlueprint(viewer.transform, xOffset, yOffset);

            if (pair.NeedsStrut)
            {
                VoxelBlueprint strutPiece = pair.WithStrut.BestBlueprint;
                BlueprintViewer strutViewer = mothership.InstantiateBlueprint(strutPiece);
                mothership.PlaceBlueprint(strutViewer.transform, xOffset, yOffset + 1);
            }
        }

        private void InstantiateNonRoofPieces()
        {
            for (int i = 0; i < nonRoofPieces.Count; i++)
            {
                CreateAndPlacePiecePair(nonRoofPieces[i], i, 0);
            }
        }
    }

    private class RoofPieceGroup
    {
        public PotentialStrutPair SlantedPiece { get; }
        public PotentialStrutPair WalkablePiece { get; }
        public IEnumerable<PotentialStrutPair> ComboPieces { get; }

        public RoofPieceGroup(VoxelBlueprint baseSlantedPiece, Dictionary<string, VoxelBlueprint> allPieces)
        {
            BlueprintContainer basePieceContainer = new BlueprintContainer(baseSlantedPiece, baseSlantedPiece);
            SlantedPiece = new PotentialStrutPair(basePieceContainer, allPieces);
            WalkablePiece = GetWalkablePiece(allPieces);
            ComboPieces = GetComboPieces();
        }

        private IEnumerable<PotentialStrutPair> GetComboPieces()
        {
            //TODO: Generate all the combo piece options
            return null;
        }

        private PotentialStrutPair GetWalkablePiece(Dictionary<string, VoxelBlueprint> allPieces)
        {
            VoxelBlueprint hypotheticalWalkablePiece = GetWalkablePieceKey();
            string key = GetInvariantKey(hypotheticalWalkablePiece);
            BlueprintContainer container;
            if (allPieces.ContainsKey(key))
            {
                container = new BlueprintContainer(hypotheticalWalkablePiece, allPieces[key]);
            }
            else
            {
                container = new BlueprintContainer(hypotheticalWalkablePiece, null);
            }
            return new PotentialStrutPair(container, allPieces);
        }

        private VoxelBlueprint GetWalkablePieceKey()
        {
            VoxelDesignationType[] baseSet = SlantedPiece.BasePiece.ExistantBlueprint.Designations.ToFlatArray();
            for (int i = 0; i < 8; i++)
            {
                if(baseSet[i] == VoxelDesignationType.SlantedRoof)
                {
                    baseSet[i] = VoxelDesignationType.WalkableRoof;
                }
            }
            DesignationGrid newGrid = DesignationGrid.FromFlatArray(baseSet);
            VoxelBlueprint ret = new VoxelBlueprint();
            ret.Designations = newGrid;
            return ret;
        }
    }

    private class BlueprintContainer
    {
        public VoxelBlueprint HypotheticalBlueprint { get; }
        public VoxelBlueprint ExistantBlueprint { get; }

        public VoxelBlueprint BestBlueprint { get { return ExistantBlueprint ?? HypotheticalBlueprint; } }

        public BlueprintContainer(VoxelBlueprint hypothetical, VoxelBlueprint existant)
        {
            HypotheticalBlueprint = hypothetical;
            ExistantBlueprint = existant;
        }
    }

    private class PotentialStrutPair
    {
        public BlueprintContainer BasePiece { get; }
        public BlueprintContainer WithStrut { get; }
        public bool NeedsStrut { get; }
        public bool HasStrut { get; }

        public PotentialStrutPair(BlueprintContainer basePiece, Dictionary<string, VoxelBlueprint> allPieces)
        {
            BasePiece = basePiece;
            NeedsStrut = GetNeedsStrut();
            if(NeedsStrut)
            {
                WithStrut = TryFindStrut(allPieces);
                HasStrut = WithStrut != null;
            }
        }

        private BlueprintContainer TryFindStrut(Dictionary<string, VoxelBlueprint> allPieces)
        {
            VoxelBlueprint strutVersion = new VoxelBlueprint();
            strutVersion.Up = VoxelConnectionType.BigStrut;
            strutVersion.Designations = BasePiece.HypotheticalBlueprint.Designations;

            string invariantKey = GetInvariantKey(strutVersion);
            if (allPieces.ContainsKey(invariantKey))
                return new BlueprintContainer(strutVersion, allPieces[invariantKey]);
            else
                return new BlueprintContainer(strutVersion, null);
        }

        // A piece needs a strut when none of the designations on the top half are filled
        private bool GetNeedsStrut()
        {
            return BasePiece.BestBlueprint.Designations.X0Y1Z0 != VoxelDesignationType.AnyFilled
                && BasePiece.BestBlueprint.Designations.X0Y1Z1 != VoxelDesignationType.AnyFilled
                && BasePiece.BestBlueprint.Designations.X1Y1Z0 != VoxelDesignationType.AnyFilled
                && BasePiece.BestBlueprint.Designations.X1Y1Z1 != VoxelDesignationType.AnyFilled;
        }
    }
}
