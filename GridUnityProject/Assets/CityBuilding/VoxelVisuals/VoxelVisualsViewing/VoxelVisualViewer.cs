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

    private List<BlueprintViewer> blueprintViewers;

    public float Margin;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        IEnumerable<VoxelBlueprint> allBlueprints = VoxelBlueprint.GetAllBlueprints();
        OrganizedBlueprints visuals = new OrganizedBlueprints(this, allBlueprints);
        blueprintViewers = new List<BlueprintViewer>();
        visuals.InstantiateGameObjects();
        Report();
    }


    //This code doesn't work right
    //private void SaveAllBlueprints()
    //{
    //    foreach (BlueprintViewer viewer in blueprintViewers)
    //    {
    //        string path = viewer.GetCorrectAssetPath();
    //        string[] foundAsset = AssetDatabase.FindAssets(path);
    //        if(foundAsset.Length == 0)
    //        {
    //            viewer.StubBlueprintFromCurrent();
    //        }
    //    }
    //}

    private void Report()
    {
        int missing = blueprintViewers.Count(item => item.Blueprint.ArtContent == null);
        Debug.Log(blueprintViewers.Count + " blueprints, with " + missing + " missing.");
    }

    public BlueprintViewer InstantiateBlueprint(VoxelBlueprint blueprint)
    {
        GameObject gameObj = Instantiate(BlueprintViewerPrefab);
        BlueprintViewer ret = gameObj.GetComponent<BlueprintViewer>();
        ret.GeneratedName = blueprint.GetCorrectAssetName();
        ret.Blueprint = blueprint;
        blueprintViewers.Add(ret);
        return ret;
    }

    public void PlaceBlueprint(Transform transform, int xIndex, float yIndex)
    {
        transform.position = new Vector3(-xIndex * (1f + Margin), 0, -yIndex * (1f + Margin));
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
                for (int comboIndex = 0; comboIndex < group.ComboPieces.Length; comboIndex++)
                {
                    CreateAndPlacePiecePair(group.ComboPieces[comboIndex], i, comboIndex * 2 + 6);
                }
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
                mothership.PlaceBlueprint(strutViewer.transform, xOffset, yOffset + .75f);
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
        private readonly Dictionary<string, VoxelBlueprint> allPieces;
        public PotentialStrutPair SlantedPiece { get; }
        public PotentialStrutPair WalkablePiece { get; }
        public PotentialStrutPair[] ComboPieces { get; }

        public RoofPieceGroup(VoxelBlueprint baseSlantedPiece, Dictionary<string, VoxelBlueprint> allPieces)
        {
            this.allPieces = allPieces;
            BlueprintContainer basePieceContainer = new BlueprintContainer(baseSlantedPiece, baseSlantedPiece);
            SlantedPiece = new PotentialStrutPair(basePieceContainer, allPieces);
            VoxelBlueprint hypotheticalWalkablePiece = GetWalkablePieceKey();
            WalkablePiece = GetForBlueprint(hypotheticalWalkablePiece);
            ComboPieces = GetComboPieces().ToArray();
        }

        private IEnumerable<PotentialStrutPair> GetComboPieces()
        {
            VoxelDesignationType[][] mixedCombos = GetMixedComboDesignations();
            for (int i = 0; i < mixedCombos.Length; i++)
            {
                VoxelBlueprint blueprint = new VoxelBlueprint();
                DesignationGrid grid = DesignationGrid.FromFlatArray(mixedCombos[i]);
                blueprint.Designations = grid;
                yield return GetForBlueprint(blueprint);
            }
        }

        private PotentialStrutPair GetForBlueprint(VoxelBlueprint hypotheticalBlueprint)
        {
            string key = GetInvariantKey(hypotheticalBlueprint);
            BlueprintContainer container;
            if (allPieces.ContainsKey(key))
            {
                container = new BlueprintContainer(hypotheticalBlueprint, allPieces[key]);
            }
            else
            {
                container = new BlueprintContainer(hypotheticalBlueprint, null);
            }
            return new PotentialStrutPair(container, allPieces);
        }

        private VoxelDesignationType[][] GetMixedComboDesignations()
        {
            VoxelDesignationType[] baseDesignation = SlantedPiece.BasePiece.BestBlueprint.Designations.ToFlatArray();
            IEnumerable<VoxelDesignationType[]> allCombos = GetAllPossibleDesignationKeys(baseDesignation, 0);
            IEnumerable<VoxelDesignationType[]> onlyMixed = allCombos.Where(set => set.Any(item => item == VoxelDesignationType.SlantedRoof) && set.Any(item => item == VoxelDesignationType.WalkableRoof)).ToArray();
            IEnumerable<VoxelDesignationType[]> onlyUnique = GetOnlyUniqueDesignations(onlyMixed);
            return onlyUnique.ToArray();
        }

        private IEnumerable<VoxelDesignationType[]> GetOnlyUniqueDesignations(IEnumerable<VoxelDesignationType[]> onlyMixed)
        {
            Dictionary<string, VoxelDesignationType[]> dictionary = new Dictionary<string, VoxelDesignationType[]>();
            foreach (VoxelDesignationType[] designation in onlyMixed)
            {
                VoxelBlueprint blueprint = new VoxelBlueprint();
                blueprint.Designations = DesignationGrid.FromFlatArray(designation);
                string key = GetInvariantKey(blueprint);
                if(!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, designation);
                }
            }
            return dictionary.Values;
        }

        // For every "Slanted" slot, yield a version that is is also walkable in that slot  
        private static IEnumerable<VoxelDesignationType[]> GetAllPossibleDesignationKeys(VoxelDesignationType[] currentDesignation, int iStart)
        {
            for (int i = iStart; i < 8; i++)
            {
                if (currentDesignation[i] == VoxelDesignationType.SlantedRoof)
                {
                    VoxelDesignationType[] cloned = currentDesignation.Clone() as VoxelDesignationType[];
                    cloned[i] = VoxelDesignationType.WalkableRoof;
                    foreach (VoxelDesignationType[] item in GetAllPossibleDesignationKeys(cloned, i + 1))
                    {
                        yield return item;
                    }
                }
            }
            yield return currentDesignation;
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
