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
            IEnumerable<string> keys = option.GetDesignationKeys();
            foreach (string key in keys)
            {
                if(lists.ContainsKey(key))
                {
                    lists[key].Add(option);
                }
                else
                {
                    lists.Add(key, new List<VoxelVisualOption>() { option });
                }
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