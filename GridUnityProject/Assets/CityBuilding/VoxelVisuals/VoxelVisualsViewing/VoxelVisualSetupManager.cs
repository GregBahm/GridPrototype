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
    public MasterVisualSetup VisualSetup => visualSetup;

    [SerializeField]
    private VoxelVisualComponent[] sourceComponents;
    public VoxelVisualComponent[] SourceComponents => sourceComponents;

    [SerializeField]
    private Material[] componentMaterials;

    
#if (UNITY_EDITOR) 
    private void Start()
    {

    }

    private void Update()
    {
    }
#endif
}
