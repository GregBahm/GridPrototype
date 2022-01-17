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

    private void MakeTheMegastub()
    {
        GameObject gameObjectRoot = new GameObject();
        foreach (BlueprintViewer viewer in blueprintViewers.Where(item => item.Blueprint.ArtContent != null))
        {
            viewer.MeshFilter.gameObject.name = viewer.Blueprint.name;
            viewer.MeshFilter.gameObject.transform.SetParent(gameObjectRoot.transform, true);
        }
    }

    private void GeneratePlatformStubVisuals(OrganizedBlueprints visuals)
    {
        foreach (PotentialStrutPair item in visuals.platformBlueprints)
        {
            StubPlatformPieceVisual(item.BasePiece.BestBlueprint, visuals);
            if (item.HasStrut)
            {
                StubPlatformPieceVisual(item.WithStrut.BestBlueprint, visuals);
            }
        }
    }

    private void StubPlatformPieceVisual(VoxelBlueprint blueprint, OrganizedBlueprints visuals)
    {
        PlatformPiece platformPiece = new PlatformPiece(blueprint, visuals.pieceDictionary);
        if (platformPiece.NonPlatformEquivalent != null && platformPiece.NonPlatformEquivalent.ArtContent != null)
            platformPiece.CreateStubVisual();
    }


    //This code doesn't work right. It seems to create changes that it shouldn't create.
    private void SaveAllBlueprints()
    {
        foreach (BlueprintViewer viewer in blueprintViewers)
        {
            string path = viewer.GetCorrectAssetPath();
            string[] foundAsset = AssetDatabase.FindAssets(path);
            if(foundAsset.Length == 0)
            {
                viewer.StubBlueprintFromCurrent();
            }
        }
    }

    private void Report()
    {
        int missing = blueprintViewers.Count(item => item.Blueprint.ArtContent == null && !item.Blueprint.ArtContentless);
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
        List<string> asKeys = options.Select(item
            => GetVisualCellOptionKey(item)).ToList();
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
        public readonly Dictionary<string, VoxelBlueprint> pieceDictionary;
        public readonly List<PotentialStrutPair> platformBlueprints;
        public OrganizedBlueprints(VoxelVisualViewer mothership, IEnumerable<VoxelBlueprint> allBlueprints)
        {
            this.mothership = mothership;
            this.allBlueprints = allBlueprints;
            pieceDictionary = new Dictionary<string, VoxelBlueprint>();

            this.pieceDictionary = allBlueprints.ToDictionary(item => GetInvariantKey(item), item => item);

            roofPieces = GetRoofPieceGroups().ToList();
            nonRoofPieces = GetNonRoofPieces().ToList();
            platformBlueprints = GetPlatformPieces(allBlueprints).ToList();
        }

        private IEnumerable<PotentialStrutPair> GetPlatformPieces(IEnumerable<VoxelBlueprint> allBlueprints)
        {
            foreach (VoxelBlueprint blueprint in allBlueprints.Where(item => item.Up != VoxelConnectionType.BigStrut))
            {
                VoxelDesignationType[] designations = blueprint.Designations.ToFlatArray();
                if (designations.Any(item => item == VoxelDesignationType.Platform))
                {
                    BlueprintContainer container = new BlueprintContainer(blueprint, blueprint);
                    PotentialStrutPair pair = new PotentialStrutPair(container, pieceDictionary);
                    yield return pair;
                }
            }
        }

        private IEnumerable<PotentialStrutPair> GetNonRoofPieces()
        {
            foreach (VoxelBlueprint blueprint in allBlueprints.Where(item => item.Up != VoxelConnectionType.BigStrut))
            {
                VoxelDesignationType[] designations = blueprint.Designations.ToFlatArray();
                if (designations.All(item => item != VoxelDesignationType.SlantedRoof 
                        && item != VoxelDesignationType.WalkableRoof
                        && item != VoxelDesignationType.Platform))
                {
                    BlueprintContainer container = new BlueprintContainer(blueprint, blueprint);
                    PotentialStrutPair pair = new PotentialStrutPair(container, pieceDictionary);
                    yield return pair;
                }
            }
        }

        internal void InstantiateGameObjects()
        {
            InstantiateSet(nonRoofPieces, 0);
            InstantiateRoofPieces();
            InstantiatePlatformBlueprints();
        }

        private void InstantiatePlatformBlueprints()
        {
            List<PotentialStrutPair> withSlanted = new List<PotentialStrutPair>();
            List<PotentialStrutPair> withWalkable = new List<PotentialStrutPair>();
            List<PotentialStrutPair> withBoth = new List<PotentialStrutPair>();
            List<PotentialStrutPair> withNeither = new List<PotentialStrutPair>();
            foreach (PotentialStrutPair item in platformBlueprints)
            {
                IEnumerable<VoxelDesignationType> designations = item.BasePiece.BestBlueprint.Designations.ToFlatArray();
                bool hasSlanted = designations.Any(item => item == VoxelDesignationType.SlantedRoof);
                bool hasWalkable = designations.Any(item => item == VoxelDesignationType.WalkableRoof);
                if (hasSlanted && hasWalkable)
                    withBoth.Add(item);
                else if (hasSlanted)
                    withSlanted.Add(item);
                else if (hasWalkable)
                    withWalkable.Add(item);
                else
                    withNeither.Add(item);
            }
            InstantiateSet(withNeither, -2);
            InstantiateSet(withSlanted, -4);
            InstantiateSet(withWalkable, -6);
            InstantiateSet(withBoth, -8);
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
                if (slots.Any(item => item == VoxelDesignationType.SlantedRoof) 
                    && !slots.Any(item => item == VoxelDesignationType.WalkableRoof)
                    && !slots.Any(item => item == VoxelDesignationType.Platform))
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

        private void InstantiateSet(List<PotentialStrutPair> set, int yOffset)
        {
            for (int i = 0; i < set.Count; i++)
            {
                CreateAndPlacePiecePair(set[i], i, yOffset);
            }
        }
    }

    private class PlatformPiece
    {
        public VoxelBlueprint PlatformViewer { get; }
        public VoxelBlueprint NonPlatformEquivalent { get; }

        public PlatformPiece(VoxelBlueprint platformBlueprint, Dictionary<string, VoxelBlueprint> allBlueprints)
        {
            PlatformViewer = platformBlueprint;
            string nonPlatformKey = MakeNonPlatformKey();
            if (allBlueprints.ContainsKey(nonPlatformKey))
                NonPlatformEquivalent = allBlueprints[MakeNonPlatformKey()];
        }

        private string MakeNonPlatformKey()
        {
            VoxelBlueprint blueprintForKey = new VoxelBlueprint();
            VoxelDesignationType[] baseDesignations = PlatformViewer.Designations.ToFlatArray();
            for (int i = 0; i < baseDesignations.Length; i++)
            {
                if(baseDesignations[i] == VoxelDesignationType.Platform)
                {
                    baseDesignations[i] = VoxelDesignationType.Empty;
                }
            }
            blueprintForKey.Designations = DesignationGrid.FromFlatArray(baseDesignations);
            blueprintForKey.Up = PlatformViewer.Up;
            blueprintForKey.Down = PlatformViewer.Down;
            return GetInvariantKey(blueprintForKey);
        }

        public void CreateStubVisual()
        {
            GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MeshFilter filter = baseObj.GetComponent<MeshFilter>();
            filter.mesh = NonPlatformEquivalent.ArtContent;

            var designations = PlatformViewer.Designations.ToCubedArray();
            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    if(designations[x, 0, z] == VoxelDesignationType.Platform)
                    {
                        AddCube(baseObj, x, z);
                    }
                }
            }
            ObjExporter.GameObjectToFile(baseObj, 0, "ExportTests/" + PlatformViewer.GetCorrectAssetName() + ".obj");
        }

        private void AddCube(GameObject baseObj, int x, int z)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = baseObj.transform;
            cube.transform.localScale = new Vector3(.5f, .03f, .5f);
            cube.transform.localPosition = new Vector3(x * .5f - .225f, -0.015f, z * .5f - .225f);
            cube.name = "Platform";
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
            if (BasePiece.HypotheticalBlueprint.Designations.ToFlatArray().All(item => item == VoxelDesignationType.Platform || item == VoxelDesignationType.Empty))
                strutVersion.Down = VoxelConnectionType.BigStrut;
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
