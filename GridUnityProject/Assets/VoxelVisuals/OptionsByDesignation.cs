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
        return allOptions.GroupBy(item => item.GetDesignationKey())
            .ToDictionary(item => item.Key, item => item.ToArray());
    }
}