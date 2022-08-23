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
    private Material[] componentMaterials;

    [SerializeField]
    private GameObject voxelVisualSetViewerPrefab;
    
#if (UNITY_EDITOR) 
    private void Start()
    {
        InstantiateSets();
        //visualSetup.SetInitialComponents(); // Run To set initial components
        //ProceduralBinding();
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
        Dictionary<string, VoxelVisualComponentSet> setsByDesignationKey = visualSetup.ComponentSets.ToDictionary(item => GetComponentKey(item.Designation), item => item);

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
                    if (pairFound)
                        throw new Exception("One component satisfies two sets?");
                    ComponentInSet inSet = new ComponentInSet(component, item.WasFlipped, item.Rotations);
                    setsByDesignationKey[item.Key].Components = new ComponentInSet[] { inSet};
                    pairFound = true;
                }
            }
            if (!pairFound)
                throw new Exception("Component " + component.name + " satisfies no sets");
        }
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
