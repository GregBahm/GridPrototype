using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using VoxelVisuals;

public class VoxelVisualSetupManager : MonoBehaviour
{
    [SerializeField]
    private MasterVisualSetup visualSetup;

    [SerializeField]
    private VoxelVisualComponent[] sourceComponents;

    [SerializeField]
    private Material[] componentMaterials;

    public bool DoSetInitialComponents;

#if (UNITY_EDITOR) 
    private void Start()
    {
        sourceComponents = CreateInitialVoxelVisualComponents().ToArray();
    }

    private void Update()
    {
        if(DoSetInitialComponents)
        {
            DoSetInitialComponents = false;
            visualSetup.SetInitialComponents();
        }
    }

    private string meshDir = "Assets/CityBuilding/VoxelVisuals/Components/";

    private IEnumerable<VoxelVisualComponent> CreateInitialVoxelVisualComponents()
    {
        string[] guids = AssetDatabase.FindAssets("t: GameObject", new[] { meshDir });
        List<GameObject> meshes = guids.Select(item => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(item))).ToList();


        foreach (GameObject prefab in meshes)
        {
            yield return CreateComponent(prefab);
        }
    }

    private VoxelVisualComponent CreateComponent(GameObject prefab)
    {
        string[] materialNames = prefab.GetComponent<MeshRenderer>().sharedMaterials.Select(item => item.name).ToArray();
        Mesh mesh = prefab.GetComponent<MeshFilter>().sharedMesh;

        Dictionary<string, Material> matTable = componentMaterials.ToDictionary(item => item.name, item => item);
        Material[] mats = materialNames.Select(item => matTable[item]).ToArray();

        VoxelVisualComponent component = ScriptableObject.CreateInstance<VoxelVisualComponent>();
        component.Mesh = mesh;
        component.Materials = mats;
        component.name = prefab.name;

        string componentPath = meshDir + component.name + ".asset";
        AssetDatabase.CreateAsset(component, componentPath);
        return component;
    }

#endif
}
