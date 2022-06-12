using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelVisuals
{
    public class VisualOptionsByDesignation
    {
        private Dictionary<string, VisualCellOptions> optionsByDesignationKey;
        private readonly VoxelVisualComponentSet[] componentSets;
        public IEnumerable<VoxelVisualComponentSet> ComponentSets { get { return componentSets; } }

        public VisualOptionsByDesignation(VoxelVisualComponentSet[] blueprints)
        {
            this.componentSets = blueprints;
            SetOptions();
        }

        private void SetOptions()
        {
            VisualCellOption[] allOptions = GetAllOptions(componentSets).ToArray();
            optionsByDesignationKey = GetOptionsByDesignationKey(allOptions);
        }

        public VisualCellOptions GetOptions(VoxelVisualDesignation designation)
        {
            return optionsByDesignationKey[designation.Key];
        }

        private IEnumerable<VisualCellOption> GetAllOptions(VoxelVisualComponentSet[] componetSets)
        {
            foreach (VoxelVisualComponentSet componentSet in componetSets)
            {
                IEnumerable<VisualCellOption> options = componentSet.GetAllPermutations();
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