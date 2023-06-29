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
        visualSetup.SetInitialComponents(); // Run To set initial components
        ProceduralBinding();
        viewers = InstantiateSets();
        //DebuggyBuddy("None_W_S_E_E_W_E_E_E_None", 0);
        //DebuggyBuddy("None_W_A_E_A_S_E_E_E_None", 1);
        //DebuggyBuddy("None_S_E_E_E_W_E_E_E_None", -1);
    }

    private void DebuggyBuddy(string componentName, float zOffset)
    {
        GameObject group = new GameObject(componentName);

        VoxelVisualComponentSet set = visualSetup.ComponentSets.First(item => item.Components.Any(item => item.Component.name == componentName));
        VoxelVisualDesignation designation = set.Designation.ToDesignation();
        IEnumerable<GeneratedVoxelDesignation> variants = designation.GetUniqueVariants(true);
        int i = 0;
        foreach (var variant in variants)
        {
            ComponentInSet[] variantComponents = set.GetVariantComponents(variant.Rotations, variant.WasFlipped).ToArray();
            VoxelVisualComponentSet variantSet = new VoxelVisualComponentSet(VoxelConnectionType.None, VoxelConnectionType.None, variant, variantComponents);
            GameObject setGameObject = Instantiate(voxelVisualSetViewerPrefab);
            setGameObject.transform.SetParent(group.transform, false);
            VoxelVisualSetViewer viewer = setGameObject.GetComponent<VoxelVisualSetViewer>();
            viewer.Initialize(variantSet);
            setGameObject.transform.position = new Vector3(i * 2, 0, zOffset * 2);
            setGameObject.name = "Designation Rotations " + variant.Rotations + (variant.WasFlipped ? ", Flipped" : "") + " Component Rotations : " + set.Components[0].Rotations + (set.Components[0].Flipped ? ", Flipped" : "");
            i++;
        }
    }

    private List<VoxelVisualSetViewer> InstantiateSets()
    {
        List<VoxelVisualSetViewer> viewers = new List<VoxelVisualSetViewer>();
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
        List<VoxelVisualSetViewer> noRoofs = new List<VoxelVisualSetViewer>();
        List<VoxelVisualSetViewer> flatRoofs = new List<VoxelVisualSetViewer>();
        List<VoxelVisualSetViewer> slantedRoofs = new List<VoxelVisualSetViewer>();
        List<VoxelVisualSetViewer> mixedRoofs = new List<VoxelVisualSetViewer>();

        foreach (var item in viewers)
        {
            var designation = item.Model.Designation.ToDesignation();
            bool hasFlatRoofs = GetDoesHaveRoofType(designation, Designation.SquaredWalkableRoof);
            bool hasSlantedRoofs = GetDoesHaveRoofType(designation, Designation.SquaredSlantedRoof);
            if(hasFlatRoofs)
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

        PlaceRow(noRoofs, 0);
        PlaceRow(flatRoofs, 1);
        PlaceRow(slantedRoofs, 2);
        PlaceRow(mixedRoofs, 3);
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
        return des.FlatDescription.Count(item => item == Designation.Empty);
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
