using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CombinationSolver
{
    public CombinationSolver()
    {
        var uniqueCombinations = GetAllUniqueCombinations().ToArray();
        Console.WriteLine(uniqueCombinations.Count());
    }

    private IEnumerable<VoxelDesignation> GetUniqueComponents()
    {
        yield return new EmptyDesignation();
        yield return new ShellDesignation();
        yield return new AquaductDesignation();

        yield return new CorneredSlantedRoofDesignation();
        yield return new CorneredWalkableRoofDesignation();
        yield return new RoundedWalkableRoofDesignation();
        yield return new RoundedSlantedRoofDesignation();

        yield return new CoveredPlatformDesignation();
        yield return new UncoveredPlatformDesignation();
    }

    private IEnumerable<VoxelVisualDesignation> GetAllUniqueCombinations()
    {
        VoxelDesignation[] components = GetUniqueComponents().ToArray();
        Dictionary<string, VoxelVisualDesignation> result = new Dictionary<string, VoxelVisualDesignation>(); ;
        for (int x0y0z0 = 0; x0y0z0 < components.Length; x0y0z0++)
        {
            for (int x0y0z1 = 0; x0y0z1 < components.Length; x0y0z1++)
            {
                for (int x0y1z0 = 0; x0y1z0 < components.Length; x0y1z0++)
                {
                    for (int x0y1z1 = 0; x0y1z1 < components.Length; x0y1z1++)
                    {
                        for (int x1y0z0 = 0; x1y0z0 < components.Length; x1y0z0++)
                        {
                            for (int x1y0z1 = 0; x1y0z1 < components.Length; x1y0z1++)
                            {
                                for (int x1y1z0 = 0; x1y1z0 < components.Length; x1y1z0++)
                                {
                                    for (int x1y1z1 = 0; x1y1z1 < components.Length; x1y1z1++)
                                    {
                                        VoxelDesignation[] description = new VoxelDesignation[8];
                                        description[0] = components[x0y0z0];
                                        description[1] = components[x0y0z1];
                                        description[2] = components[x0y1z0];
                                        description[3] = components[x0y1z1];
                                        description[4] = components[x1y0z0];
                                        description[5] = components[x1y0z1];
                                        description[6] = components[x1y1z0];
                                        description[7] = components[x1y1z1];
                                        
                                        VoxelVisualDesignation designation = new VoxelVisualDesignation(description).GetMasterVariant();
                                        if(!result.ContainsKey(designation.Key))
                                            result.Add(designation.Key, designation);
                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine(x0y0z0);
            }
        }
        return result.Values;
    }
}