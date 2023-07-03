using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelVisuals;
using static UnityEditor.Progress;

public class VoxelVisualSetupManager : MonoBehaviour
{
    [SerializeField]
    private MasterVisualSetup visualSetup;
    public MasterVisualSetup VisualSetup => visualSetup;

    [SerializeField]
    private VoxelVisualComponent[] sourceComponents;
    public VoxelVisualComponent[] SourceComponents => sourceComponents;

    [SerializeField]
    private VoxelVisualComponent upStructComponent;
    [SerializeField]
    private VoxelVisualComponent downStructComponent;

    [SerializeField]
    private Material[] componentMaterials;

    [SerializeField]
    private GameObject voxelVisualSetViewerPrefab;

    private List<VoxelVisualSetViewer> viewers;
    
#if (UNITY_EDITOR) 
    private void Start()
    {
        visualSetup.SetInitialComponents(); // Run To setup initial components. Will overwrite existing visual setup.
        ProceduralBinding();
        viewers = InstantiateSets();
    }

    private List<VoxelVisualSetViewer> InstantiateSets()
    {
        List<VoxelVisualSetViewer> viewers = new List<VoxelVisualSetViewer>();
        //IEnumerable<VoxelVisualComponentSet> singleSubsectionSet = SingleSubsectionSelector.Get(visualSetup.ComponentSets); // Comment out when done with this set
        //foreach (VoxelVisualComponentSet set in singleSubsectionSet)
        foreach (VoxelVisualComponentSet set in visualSetup.ComponentSets)
        {
            GameObject setGameObject = Instantiate(voxelVisualSetViewerPrefab);
            VoxelVisualSetViewer viewer = setGameObject.GetComponent<VoxelVisualSetViewer>();
            viewer.Initialize(set);
            viewers.Add(viewer);
        }
        PlaceSets(viewers);
        return viewers;
    }

    private void PlaceSets(List<VoxelVisualSetViewer> viewers)
    {
        List<VoxelVisualSetViewer> shell = new List<VoxelVisualSetViewer>();
        List<VoxelVisualSetViewer> noShell = new List<VoxelVisualSetViewer>();
        foreach (var item in viewers)
        {
            if(item.Model.Designation.ToDesignation().FlatDescription.Any(item => item == Designation.Shell))
                shell.Add(item);
            else
                noShell.Add(item);
        }
        PlaceSets(shell, 0);
        PlaceSets(noShell, 5);
    }

    private void PlaceSets(List<VoxelVisualSetViewer> viewers, int offset)
    {
        List<VoxelVisualSetViewer> onlyGrounds = new List<VoxelVisualSetViewer>();
        List<VoxelVisualSetViewer> noRoofs = new List<VoxelVisualSetViewer>();
        List<VoxelVisualSetViewer> flatRoofs = new List<VoxelVisualSetViewer>();
        List<VoxelVisualSetViewer> slantedRoofs = new List<VoxelVisualSetViewer>();
        List<VoxelVisualSetViewer> mixedRoofs = new List<VoxelVisualSetViewer>();

        foreach (VoxelVisualSetViewer item in viewers)
        {
            var designation = item.Model.Designation.ToDesignation();
            bool onlyGround = designation.FlatDescription.All(item => item == Designation.Empty || item == Designation.Shell);
            bool hasFlatRoofs = GetDoesHaveRoofType(designation, Designation.SquaredWalkableRoof);
            bool hasSlantedRoofs = GetDoesHaveRoofType(designation, Designation.SquaredSlantedRoof);
            if(onlyGround)
            {
                onlyGrounds.Add(item);
            }
            else if(hasFlatRoofs)
            {
                if(hasSlantedRoofs)
                {
                    mixedRoofs.Add(item);
                }
                else
                {
                    flatRoofs.Add(item);
                }
            }
            else if(hasSlantedRoofs)
            {
                slantedRoofs.Add(item);
            }
            else
            {
                noRoofs.Add(item);
            }
        }

        if(onlyGrounds.Any())
            PlaceRow(onlyGrounds, 0 + offset, "Only Ground");
        PlaceRow(noRoofs, 1 + offset, "No Roofs");
        PlaceRow(flatRoofs, 2 + offset, "Walkable Roofs");
        PlaceRow(slantedRoofs, 3 + offset, "Slanted Roofs");
        PlaceRow(mixedRoofs, 4 + offset, "Mixed Roofs");
    }

    private void PlaceRow(List<VoxelVisualSetViewer> set, int xOffset, string rowLabel)
    {
        GameObject rowContainer = new GameObject(rowLabel);
        VoxelVisualSetViewer[] orderedSet = set.OrderBy(item => GetRowVal(item)).ToArray();
        for (int i = 0; i < orderedSet.Length; i++)
        {
            GameObject obj = orderedSet[i].gameObject;
            obj.transform.position = new Vector3(i * 2, 0, xOffset * 2);
            obj.transform.SetParent(rowContainer.transform, false);
        }
    }

    private int GetRowVal(VoxelVisualSetViewer item)
    {
        VoxelVisualDesignation des = item.Model.Designation.ToDesignation();
        return -des.FlatDescription.Count(item => item == Designation.Empty);
    }

    private bool GetDoesHaveRoofType(VoxelVisualDesignation designation, Designation roofType)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if(designation.Description[x, 1, z] == Designation.Empty)
                {
                    if (designation.Description[x, 0, z] == roofType)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void ProceduralBinding()
    {
        ProceduralBindingBlueprints[] blueprints = visualSetup.ComponentSets.Select(item => new ProceduralBindingBlueprints(item)).ToArray();
        Dictionary<string, ComponentInSet> componentsByKey = GetComponentsByKey();
        ComponentInSet upStructSetComponent = new ComponentInSet(upStructComponent, false, 0);
        ComponentInSet downStructSetComponent = new ComponentInSet(downStructComponent, false, 0);
        
        foreach (ProceduralBindingBlueprints blueprint in blueprints)
        {
            blueprint.Bind(componentsByKey, upStructSetComponent, downStructSetComponent);
        }
    }

    private Dictionary<string, ComponentInSet> GetComponentsByKey()
    {
        Dictionary<string, ComponentInSet> ret = new Dictionary<string, ComponentInSet>();
        foreach (VoxelVisualComponent item in sourceComponents)
        {
            ProceeduralBindingComponent wrapper = new ProceeduralBindingComponent(item);
            foreach (var setItem in wrapper.ComponentsByKey)
            {
                ret.Add(setItem.Key, setItem.Value);
            }
        }
        return ret;
    }

    private class ProceeduralBindingComponent
    {
        private VoxelVisualComponent component;
        private GeneratedVoxelDesignation designationOfComponent;

        public Dictionary<string, ComponentInSet> ComponentsByKey { get; private set; }

        public ProceeduralBindingComponent(VoxelVisualComponent component)
        {
            this.component = component;
            Designation[] description = GetDesignationFromName(component);
            VoxelVisualDesignation baseDesignation = new VoxelVisualDesignation(description);
            VoxelVisualDesignation masterDesignation = baseDesignation.GetMasterVariant();
            IEnumerable<GeneratedVoxelDesignation> variants = masterDesignation.GetUniqueVariants(true);
            GeneratedVoxelDesignation designationOfMesh = variants.First(item => item.Key == baseDesignation.Key);
            ComponentsByKey = GetComponentSetVariations(designationOfMesh);
        }

        private Dictionary<string, ComponentInSet> GetComponentSetVariations(GeneratedVoxelDesignation designationOfMesh)
        {
            VoxelVisualComponentSet set = new VoxelVisualComponentSet(
                VoxelConnectionType.None,
                VoxelConnectionType.None,
                designationOfMesh, new ComponentInSet[] { new ComponentInSet(component, false, 0) });
            VisualCellOption[] options = set.GetAllPermutations().ToArray();
            Dictionary<string, ComponentInSet> ret = new Dictionary<string, ComponentInSet>();
            foreach (VisualCellOption item in options)
            {
                string key = item.Designation.Key;
                ComponentInSet setComponent = item.Components[0];
                ret.Add(key, setComponent);
            }
            return ret;
        }

        private Designation[] GetDesignationFromName(VoxelVisualComponent component)
        {
            string name = component.name;
            string[] components = name.Replace("None_", "").Replace("_None", "").Replace("BigStrut_", "").Replace("_BigStrut", "").Split('_');
            return components.Select(DesignationFromLetter).ToArray();
        }

        private Designation DesignationFromLetter(string letter)
        {
            switch (letter)
            {
                case "E":
                    return Designation.Empty;
                case "W":
                case "A":
                return Designation.SquaredWalkableRoof;
                case "S":
                    return Designation.SquaredSlantedRoof;
                case "G":
                    return Designation.Shell;
                default:
                    throw new Exception("typo in export?");
            }
        }
    }

    private class ProceduralBindingBlueprints
    {
        private readonly VoxelVisualComponentSet set;
        private readonly VoxelVisualDesignation groundOnlyDesignation;
        private readonly VoxelVisualDesignation noGroundDesignation;

        public ProceduralBindingBlueprints(VoxelVisualComponentSet set)
        {
            this.set = set;
            Designation[] baseDescription = set.Designation.ToDesignation().FlatDescription.ToArray();
            groundOnlyDesignation = GetGroundOnlyDesignation(baseDescription);
            noGroundDesignation = GetNoGroundDesignation(baseDescription);
        }

        public void Bind(Dictionary<string, ComponentInSet> components, ComponentInSet upStruct, ComponentInSet downStruct)
        {
            List<ComponentInSet> newComponents = new List<ComponentInSet>();
            if(components.ContainsKey(groundOnlyDesignation.Key))
            {
                newComponents.Add(components[groundOnlyDesignation.Key]);
            }
            if(components.ContainsKey(noGroundDesignation.Key))
            {
                newComponents.Add(components[noGroundDesignation.Key]);
            }
            //if (set.Up == VoxelConnectionType.BigStrut) // TODO: Uncomment when getting back into struts
            //{
            //    components.Add(upStruct);
            //}
            //if (set.Down == VoxelConnectionType.BigStrut)
            //{
            //    components.Add(downStruct);
            //}
            set.Components = newComponents.ToArray();
        }

        private VoxelVisualDesignation GetNoGroundDesignation(Designation[] baseDescription)
        {
            Designation[] ret = new Designation[8];
            for (int i = 0; i < 8; i++)
            {
                ret[i] = baseDescription[i] == Designation.Shell ? Designation.Empty : baseDescription[i];
            }
            return new VoxelVisualDesignation(ret);
        }

        private VoxelVisualDesignation GetGroundOnlyDesignation(Designation[] baseDescription)
        {
            Designation[] ret = new Designation[8];
            for (int i = 0; i < 8; i++)
            {
                ret[i] = baseDescription[i] == Designation.Shell ? Designation.Shell : Designation.Empty;
            }
            return new VoxelVisualDesignation(ret);
        }
    }
#endif
}

// A componetization strategy. Count the number of contigious empty cells in a designation. Solve all the designations that only have 1.
// Then later, you can solve all the designations that have more, from the 1s
// Intended to be a 1-off function that can be archived away later
public static class EmptyVoxelSpaceFinder
{

    public static IEnumerable<VoxelVisualComponentSet> GetVoxelsWithContiguousEmptySpace(VoxelVisualComponentSet[] componentSets)
    {
        foreach (VoxelVisualComponentSet baseSet in componentSets)
        {
            VoxelVisualDesignation voxelDesignation = baseSet.Designation.ToDesignation();
            VoxelDesignationWrapper asSubGroup = new VoxelDesignationWrapper(voxelDesignation);
            if(asSubGroup.SubGroupCount == 1)
            {
                yield return baseSet;
            }
        }
    }

    private class VoxelDesignationWrapper
    {
        public int SubGroupCount { get; }

        private DesignationWrapper[,,] subSelectors;

        public VoxelDesignationWrapper(VoxelVisualDesignation source)
        {
            subSelectors = GetSubSelectors(source);
            SubGroupCount = GetSubGroupCount();
        }

        private int GetSubGroupCount()
        {
            int ret = 0;
            IEnumerable<DesignationWrapper> empties = GetSubSelectorsAsFlatList().Where(item => item.Designation == Designation.Empty);
            HashSet<DesignationWrapper> emptiesHash = new HashSet<DesignationWrapper>(empties);
            while(emptiesHash.Any())
            {
                ret++;
                DesignationWrapper current = emptiesHash.First();
                MrRecursy(emptiesHash, current);
            }
            return ret;
        }

        private void MrRecursy(HashSet<DesignationWrapper> emptiesHash, DesignationWrapper current)
        {
            emptiesHash.Remove(current);

            IEnumerable<DesignationWrapper> adjacent = current.GetAdjacentDesignations(subSelectors);
            foreach (var item in adjacent)
            {
                if (emptiesHash.Contains(item))
                {
                    MrRecursy(emptiesHash, item);
                }
            }
        }

        private IEnumerable<DesignationWrapper> GetSubSelectorsAsFlatList()
        {
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        yield return subSelectors[x, y, z];
                    }
                }
            }
        }

        private DesignationWrapper[,,] GetSubSelectors(VoxelVisualDesignation source)
        {
            DesignationWrapper[,,] ret = new DesignationWrapper[2, 2, 2];
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        ret[x, y, z] = new DesignationWrapper(x, y, z, source.Description[x, y, z]);
                    }
                }
            }
            return ret;
        }
    }

    private class DesignationWrapper
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public Designation Designation { get; }

        public DesignationWrapper(int x, int y, int z, Designation designation)
        {
            X = x;
            Y = y; 
            Z = z;
            Designation = designation;
        }

        public DesignationWrapper[] GetAdjacentDesignations(DesignationWrapper[,,] subSelectors)
        {
            int adjacentX = X == 0 ? 1 : 0;
            int adjacentY = Y == 0 ? 1 : 0;
            int adjacentZ = Z == 0 ? 1 : 0;
            return new[] { subSelectors[adjacentX, Y, Z], subSelectors[X, adjacentY, Z], subSelectors[X, Y, adjacentZ] }; 
        }
    }
}