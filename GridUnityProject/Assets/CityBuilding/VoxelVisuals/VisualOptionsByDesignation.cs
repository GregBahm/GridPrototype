using System.Collections.Generic;
using System.Linq;

public class VisualOptionsByDesignation
{
    private Dictionary<string, VisualCellOption[]> optionsByDesignationKey;
    private readonly VoxelBlueprint[] blueprints;
    public VisualOptionsByDesignation(VoxelBlueprint[] blueprints)
    {
        this.blueprints = blueprints;
        SetOptions();
    }

    private void SetOptions()
    {
        VisualCellOption[] allOptions = GetAllOptions(blueprints).ToArray();
        optionsByDesignationKey = GetOptionsByDesignationKey(allOptions);
    }

    public VisualCellOption[] GetOptions(VoxelDesignation designation)
    {
        if (!optionsByDesignationKey.ContainsKey(designation.Key))
        {
            SetOptions();
        }
            if (!optionsByDesignationKey.ContainsKey(designation.Key))
        {
            UnityEngine.Debug.LogError("Wanted but couldn't find this key:\n" + designation.Key);
            CityBuildingMain.Instance.StubMissingBlueprint(designation);
        }
        return optionsByDesignationKey[designation.Key];
    }

    private IEnumerable<VisualCellOption> GetAllOptions(VoxelBlueprint[] blueprints)
    {
        foreach (VoxelBlueprint blueprint in blueprints)
        {
            IEnumerable<VisualCellOption> options = blueprint.GenerateVisualOptions();
            foreach (VisualCellOption option in options)
            {
                yield return option;
            }
        }
    }

    private Dictionary<string, VisualCellOption[]> GetOptionsByDesignationKey(VisualCellOption[] allOptions)
    {
        Dictionary<string, List<VisualCellOption>> lists = new Dictionary<string, List<VisualCellOption>>();
        foreach (VisualCellOption option in allOptions)
        {
            string key = option.GetDesignationKey();
            if (lists.ContainsKey(key))
            {
                lists[key].Add(option);
            }
            else
            {
                lists.Add(key, new List<VisualCellOption>() { option });
            }
        }
        Dictionary<string, VisualCellOption[]> ret = new Dictionary<string, VisualCellOption[]>();
        foreach (var item in lists)
        {
            ret.Add(item.Key, item.Value.ToArray());
        }
        return ret;
    }
}