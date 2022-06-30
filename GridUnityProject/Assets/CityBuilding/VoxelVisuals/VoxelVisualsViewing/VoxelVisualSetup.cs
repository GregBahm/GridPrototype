using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VoxelVisuals;

public class VoxelVisualSetup : MonoBehaviour
{
    [SerializeField]
    private VoxelVisualComponentSet[] ComponentSets;

    [SerializeField]
    private VoxelVisualComponent[] SourceComponents;

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
            SetInitialComponents();
        }
    }

    private void SetInitialComponents()
    {
        List<VoxelVisualComponentSet> sets = new List<VoxelVisualComponentSet>();
        CombinationSolver solver = new CombinationSolver();
        foreach (VoxelVisualDesignation item in solver.Combos)
        {
            VoxelVisualComponentSet set = new VoxelVisualComponentSet(VoxelConnectionType.None, VoxelConnectionType.None, item, new ComponentInSet[0]);
            sets.Add(set);
        }
        ComponentSets = sets.ToArray();
    }
#endif
}