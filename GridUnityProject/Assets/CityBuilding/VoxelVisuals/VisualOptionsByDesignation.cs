using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelVisuals
{
    public class VisualOptionsByDesignation
    {
        private Dictionary<string, VisualCellOptions> optionsByDesignationKey;
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

        public VisualCellOptions GetOptions(VoxelDesignation designation)
        {
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

        private Dictionary<string, VisualCellOptions> GetOptionsByDesignationKey(VisualCellOption[] allOptions)
        {
            Dictionary<string, VisualCellOptions> ret = new Dictionary<string, VisualCellOptions>();
            IEnumerable<IGrouping<string, VisualCellOption>> groups = allOptions.GroupBy(item => item.GetDesignationKey());
            foreach (IGrouping<string, VisualCellOption> group in groups)
            {
                VisualCellOption[] asArray = group.ToArray();
                if (asArray.Length > 2)
                {
                    throw new Exception("More than two options for " + group.Key);
                }
                VisualCellOptions options = new VisualCellOptions();
                foreach (VisualCellOption item in asArray)
                {
                    if (item.Connections.Up == VoxelConnectionType.BigStrut)
                        options.UpStrutOption = item;
                    else
                        options.DefaultOption = item;
                }
                ret.Add(group.Key, options);
            }
            return ret;
        }
    }

    public class VisualCellOptions
    {
        public VisualCellOption DefaultOption { get; set; }
        public VisualCellOption UpStrutOption { get; set; }
    }
}