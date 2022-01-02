using System.Collections.Generic;
using System.Linq;

public class OptionsByDesignation
{
    private readonly Dictionary<string, VoxelVisualOption[]> optionsByDesignationKey;
    public OptionsByDesignation(VoxelBlueprint[] blueprints)
    {
        VoxelVisualOption[] allOptions = GetAllOptions(blueprints).ToArray();
        optionsByDesignationKey = GetOptionsByDesignationKey(allOptions);
    }

    public VoxelVisualOption[] GetOptions(VoxelDesignation designation)
    {
        if (!optionsByDesignationKey.ContainsKey(designation.Key))
        {
            UnityEngine.Debug.LogError("Wanted but couldn't find this key:\n" + designation.Key);
        }
        return optionsByDesignationKey[designation.Key];
    }

    private IEnumerable<VoxelVisualOption> GetAllOptions(VoxelBlueprint[] blueprints)
    {
        foreach (VoxelBlueprint blueprint in blueprints)
        {
            IEnumerable<VoxelVisualOption> options = blueprint.GenerateVisualOptions();
            foreach (VoxelVisualOption option in options)
            {
                yield return option;
            }
        }
    }

    private Dictionary<string, VoxelVisualOption[]> GetOptionsByDesignationKey(VoxelVisualOption[] allOptions)
    {
        Dictionary<string, List<VoxelVisualOption>> lists = new Dictionary<string, List<VoxelVisualOption>>();
        foreach (VoxelVisualOption option in allOptions)
        {
            string key = option.GetDesignationKey();
            if (lists.ContainsKey(key))
            {
                lists[key].Add(option);
            }
            else
            {
                lists.Add(key, new List<VoxelVisualOption>() { option });
            }
        }
        Dictionary<string, VoxelVisualOption[]> ret = new Dictionary<string, VoxelVisualOption[]>();
        foreach (var item in lists)
        {
            ret.Add(item.Key, item.Value.ToArray());
        }
        return ret;
    }
}