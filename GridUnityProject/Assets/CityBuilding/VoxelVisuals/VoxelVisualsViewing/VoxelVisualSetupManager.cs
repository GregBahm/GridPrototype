using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using VoxelVisuals;

public class VoxelVisualSetupManager : MonoBehaviour
{
    [SerializeField]
    private MasterVisualSetup visualSetup;

    [SerializeField]
    private VoxelVisualComponent[] sourceComponents;

    public bool DoSetInitialComponents;

#if (UNITY_EDITOR) 
    private void Start()
    {
    }

    private void Update()
    {
        if(DoSetInitialComponents)
        {
            DoSetInitialComponents = false;
            visualSetup.SetInitialComponents();
        }
    }

#endif
}
