using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[CreateAssetMenu(menuName = "MasterVisualSetup")]
public class MasterVisualSetup : ScriptableObject
{
    [SerializeField]
    private VoxelVisualComponentSet[] componentSets;
    public VoxelVisualComponentSet[] ComponentSets => componentSets;

    public void SetInitialComponents()
    {
        List<VoxelVisualComponentSet> sets = new List<VoxelVisualComponentSet>();
        CombinationSolver solver = new CombinationSolver();
        foreach (VoxelVisualDesignation item in solver.Combos)
        {
            foreach (var set in GetStrutComponentSet(item))
            {
                sets.Add(set);
            }
        }
        componentSets = sets.ToArray();
    }

    private IEnumerable<VoxelVisualComponentSet> GetStrutComponentSet(VoxelVisualDesignation designation)
    {
        yield return new VoxelVisualComponentSet(VoxelConnectionType.None, VoxelConnectionType.None, designation, new ComponentInSet[0]);
        bool strutUp = HasUpStrut(designation);
        if(strutUp)
            yield return new VoxelVisualComponentSet(VoxelConnectionType.BigStrut, VoxelConnectionType.None, designation, new ComponentInSet[0]);
    }

    private static bool HasUpStrut(VoxelVisualDesignation designation)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                Designation des = designation.Description[x, 1, z];
                if (des != Designation.Empty)
                    return false;
            }
        }
        return true;
    }
}