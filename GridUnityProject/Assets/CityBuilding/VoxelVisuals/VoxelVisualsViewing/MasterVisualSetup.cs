using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MasterVisualSetup")]
public class MasterVisualSetup : ScriptableObject
{
    [SerializeField]
    private VoxelVisualComponentSet[] componentSets;

    public void SetInitialComponents()
    {
        List<VoxelVisualComponentSet> sets = new List<VoxelVisualComponentSet>();
        CombinationSolver solver = new CombinationSolver();
        foreach (VoxelVisualDesignation item in solver.Combos)
        {
            VoxelVisualComponentSet set = new VoxelVisualComponentSet(VoxelConnectionType.None, VoxelConnectionType.None, item, new ComponentInSet[0]);
            sets.Add(set);
        }
        componentSets = sets.ToArray();
    }
}