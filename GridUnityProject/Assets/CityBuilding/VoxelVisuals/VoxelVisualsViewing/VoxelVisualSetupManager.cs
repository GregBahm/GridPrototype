using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using VoxelVisuals;

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
    
#if (UNITY_EDITOR) 
    private void Start()
    {

        //visualSetup.SetInitialComponents(); // Run To set initial components
        ProceduralBinding();
        InstantiateSets();

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

    private void InstantiateSets()
    {
        int i = 0;
        foreach (VoxelVisualComponentSet set in visualSetup.ComponentSets)
        {
            if(set.Components.Any())
            {
                GameObject setGameObject = Instantiate(voxelVisualSetViewerPrefab);
                VoxelVisualSetViewer viewer = setGameObject.GetComponent<VoxelVisualSetViewer>();
                viewer.Initialize(set);
                setGameObject.transform.position = GetSetPosition(i);
                i++;
            }
        }
    }

    private Vector3 GetSetPosition(int i)
    {
        int rowLength = (int)Mathf.Sqrt(visualSetup.ComponentSets.Length);
        float x = i % rowLength * 2;
        float z = Mathf.FloorToInt(i / rowLength) * 2;
        return new Vector3(x, 0, z);
    }

    private void ProceduralBinding()
    {
        Dictionary<string, List<VoxelVisualComponentSet>> setsByDesignationKey = GetSetsByDesignation();

        ComponentInSet upStructSetComponent = new ComponentInSet(upStructComponent, false, 0);
        ComponentInSet downStructSetComponent = new ComponentInSet(downStructComponent, false, 0);

        // So the idea here is that I take my list of source components, and I determine the VisualSetup.Set that they belong to. Then I set them. That way, I can restore the build. Then more thoughtfully componetize, solve the ground set, and move forward.
        foreach (VoxelVisualComponent component in sourceComponents)
        {
            bool pairFound = false;
            VoxelVisualDesignation designation = new VoxelVisualDesignation(GetDesignationFromName(component));
            GeneratedVoxelDesignation[] generatedDesignations = designation.GetUniqueVariants(true).ToArray();
            foreach (GeneratedVoxelDesignation item in generatedDesignations)
            {
                if(setsByDesignationKey.ContainsKey(item.Key))
                {
                    //TODO: Loop through the sets, add their corrosponding component, and then add the struct piece
                    if (pairFound)
                        throw new Exception("One component satisfies two sets?");
                    ComponentInSet inSet = new ComponentInSet(component, item.WasFlipped, item.Rotations);
                    List<VoxelVisualComponentSet> sets = setsByDesignationKey[item.Key];
                    foreach (VoxelVisualComponentSet set in sets)
                    {
                        List<ComponentInSet> components = new List<ComponentInSet> { inSet };
                        if (set.Up == VoxelConnectionType.BigStrut)
                        {
                            components.Add(upStructSetComponent);
                        }
                        if(set.Down == VoxelConnectionType.BigStrut)
                        {
                            components.Add(downStructSetComponent);
                        }
                        set.Components = components.ToArray();
                    }
                    pairFound = true;
                }
            }
            if (!pairFound)
                throw new Exception("Component " + component.name + " satisfies no sets");
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

    private Designation[] GetDesignationFromName(VoxelVisualComponent component)
    {
        string name = component.name;
        string[] components = name.Replace("None_", "").Replace("_None", "").Split('_');
        return components.Select(DesignationFromLetter).ToArray();
    }

    private Designation DesignationFromLetter(string letter)
    {
        switch (letter)
        {
            case "A":
                return Designation.SquaredWalkableRoof;
            case "E":
                return Designation.Empty;
            case "W":
                return Designation.SquaredWalkableRoof;
            case "S":
                return Designation.SquaredSlantedRoof;
            default:
                throw new Exception("jigga what");
        }
    }
#endif
}
