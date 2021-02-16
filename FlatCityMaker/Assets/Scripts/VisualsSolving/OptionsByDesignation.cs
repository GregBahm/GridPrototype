using System.Collections.Generic;
using System.Linq;
using TileDefinition;

namespace VisualsSolver
{
    public class OptionsByDesignation
    {
        private readonly Dictionary<string, Tile[]> optionsByDesignationKey;
        public OptionsByDesignation(Tile[] tiles)
        {
            optionsByDesignationKey = GetOptionsByDesignationKey(tiles);
        }

        public Tile[] GetOptions(VoxelDesignation designation)
        {
            return optionsByDesignationKey[designation.Key];
        }

        private Dictionary<string, Tile[]> GetOptionsByDesignationKey(Tile[] allOptions)
        {
            return allOptions.GroupBy(item => item.GetDesignationKey())
                .ToDictionary(item => item.Key, item => item.ToArray());
        }
    }
}