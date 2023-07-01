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
        IEnumerable<VoxelVisualComponentSet> singleSubsectionSet = SingleSubsectionSelector.Get(visualSetup.ComponentSets); // Comment out when done with this set
        foreach (VoxelVisualComponentSet set in singleSubsectionSet)
        //foreach (VoxelVisualComponentSet set in visualSetup.ComponentSets)
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

        PlaceRow(onlyGrounds, 0 + offset);
        PlaceRow(noRoofs, 1 + offset);
        PlaceRow(flatRoofs, 2 + offset);
        PlaceRow(slantedRoofs, 3 + offset);
        PlaceRow(mixedRoofs, 4 + offset);
    }

    private void PlaceRow(List<VoxelVisualSetViewer> set, int xOffset)
    {
        VoxelVisualSetViewer[] orderedSet = set.OrderBy(item => GetRowVal(item)).ToArray();
        for (int i = 0; i < orderedSet.Length; i++)
        {
            GameObject obj = orderedSet[i].gameObject;
            obj.transform.position = new Vector3(i * 2, 0, xOffset * 2);
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
        Dictionary<string, List<VoxelVisualComponentSet>> setsByDesignationKey = GetSetsByDesignation();

        ComponentInSet upStructSetComponent = new ComponentInSet(upStructComponent, false, 0);
        ComponentInSet downStructSetComponent = new ComponentInSet(downStructComponent, false, 0);

        foreach (VoxelVisualComponent component in sourceComponents)
        {
            ProceeduralBindingComponent bindingComponent = new ProceeduralBindingComponent(component);
            bindingComponent.Bind(setsByDesignationKey, upStructSetComponent, downStructSetComponent);
        }
    }

    private class ProceeduralBindingComponent
    {
        private VoxelVisualComponent component;
        public VoxelVisualComponent Component { get { return component; } }
        private VoxelVisualDesignation masterDesignation;
        private GeneratedVoxelDesignation componentDesignation;
        public GeneratedVoxelDesignation ComponentDesignation { get { return componentDesignation; } }

        public ProceeduralBindingComponent(VoxelVisualComponent component)
        {
            this.component = component;
            Designation[] description = GetDesignationFromName(component);
            VoxelVisualDesignation baseDesignation = new VoxelVisualDesignation(description);
            masterDesignation = baseDesignation.GetMasterVariant();
            IEnumerable<GeneratedVoxelDesignation> variants = masterDesignation.GetUniqueVariants(true);
            componentDesignation = variants.First(item => item.Key == baseDesignation.Key);
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
                default:
                    throw new Exception("typo in export?");
            }
        }

        public void Bind(Dictionary<string, List<VoxelVisualComponentSet>> setsByDesignationKey, ComponentInSet upStruct, ComponentInSet downStruct)
        {
            List<VoxelVisualComponentSet> sets = setsByDesignationKey[masterDesignation.Key];

            foreach (VoxelVisualComponentSet set in sets)
            {
                ComponentInSet inSet = new ComponentInSet(component, componentDesignation.WasFlipped, componentDesignation.Rotations);
                List<ComponentInSet> components = new List<ComponentInSet> { inSet };
                if (set.Up == VoxelConnectionType.BigStrut)
                {
                    components.Add(upStruct);
                }
                if (set.Down == VoxelConnectionType.BigStrut)
                {
                    components.Add(downStruct);
                }
                set.Components = components.ToArray();
            }
        }
    }

    private Dictionary<string, List<VoxelVisualComponentSet>> GetSetsByDesignation()
    {
        var ret = new Dictionary<string, List<VoxelVisualComponentSet>>();
        foreach (var item in visualSetup.ComponentSets)
        {
            string key = GetComponentKey(item.Designation);
            if(ret.ContainsKey(key))
                ret[key].Add(item);
            else
                ret.Add(key, new List<VoxelVisualComponentSet> { item });
        }
        return ret;
    }

    private string GetComponentKey(SerializableVisualDesignation designation)
    { 
        return designation.ToDesignation().Key;
    }
#endif
}

// A componetization strategy. Count the number of contigious empty cells in a designation. Solve all the designations that only have 1.
// Then later, you can solve all the designations that have more, from the 1s
// Intended to be a 1-off function that can be archived away later
public static class SingleSubsectionSelector
{

    public static IEnumerable<VoxelVisualComponentSet> Get(VoxelVisualComponentSet[] componentSets)
    {
        foreach (VoxelVisualComponentSet baseSet in componentSets)
        {
            VoxelVisualDesignation voxelDesignation = baseSet.Designation.ToDesignation();
            SubSelectorGroup asSubGroup = new SubSelectorGroup(voxelDesignation);
            if(asSubGroup.SubGroupCount == 1)
            {
                yield return baseSet;
            }
        }
    }

    private class SubSelectorGroup
    {
        public int SubGroupCount { get; }

        private SubSelector[,,] subSelectors;

        public SubSelectorGroup(VoxelVisualDesignation source)
        {
            subSelectors = GetSubSelectors(source);
            SubGroupCount = GetSubGroupCount();
        }

        private int GetSubGroupCount()
        {
            int ret = 0;
            IEnumerable<SubSelector> empties = GetSubSelectorsAsFlatList().Where(item => item.Designation == Designation.Empty);
            HashSet<SubSelector> emptiesHash = new HashSet<SubSelector>(empties);
            while(emptiesHash.Any())
            {
                ret++;
                SubSelector current = emptiesHash.First();
                MrRecursy(emptiesHash, current);
            }
            return ret;
        }

        private void MrRecursy(HashSet<SubSelector> emptiesHash, SubSelector current)
        {
            emptiesHash.Remove(current);

            IEnumerable<SubSelector> adjacent = current.GetAdjacentDesignations(subSelectors);
            foreach (var item in adjacent)
            {
                if (emptiesHash.Contains(item))
                {
                    MrRecursy(emptiesHash, item);
                }
            }
        }

        private IEnumerable<SubSelector> GetSubSelectorsAsFlatList()
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

        private SubSelector[,,] GetSubSelectors(VoxelVisualDesignation source)
        {
            SubSelector[,,] ret = new SubSelector[2, 2, 2];
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        ret[x, y, z] = new SubSelector(x, y, z, source.Description[x, y, z]);
                    }
                }
            }
            return ret;
        }
    }

    private class SubSelector
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public Designation Designation { get; }

        public SubSelector(int x, int y, int z, Designation designation)
        {
            X = x;
            Y = y; 
            Z = z;
            Designation = designation;
        }

        public SubSelector[] GetAdjacentDesignations(SubSelector[,,] subSelectors)
        {
            int adjacentX = X == 0 ? 1 : 0;
            int adjacentY = Y == 0 ? 1 : 0;
            int adjacentZ = Z == 0 ? 1 : 0;
            return new[] { subSelectors[adjacentX, Y, Z], subSelectors[X, adjacentY, Z], subSelectors[X, Y, adjacentZ] }; 
        }
    }
}