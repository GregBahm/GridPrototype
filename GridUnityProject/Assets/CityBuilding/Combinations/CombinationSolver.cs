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
        VoxelVisualDesignation[] combos = GetAllUniqueCombinations().ToArray();
        Console.WriteLine(combos.Length);
    }


    private IEnumerable<VoxelVisualDesignation> GetAllUniqueCombinations()
    {
        Designation[] components = Designation.AllBaseDesignations.ToArray();
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
                                        Designation[] description = new Designation[8];
                                        description[0] = components[x0y0z0];
                                        description[1] = components[x0y0z1];
                                        description[2] = components[x0y1z0];
                                        description[3] = components[x0y1z1];
                                        description[4] = components[x1y0z0];
                                        description[5] = components[x1y0z1];
                                        description[6] = components[x1y1z0];
                                        description[7] = components[x1y1z1];

                                        VoxelVisualDesignation designation = new VoxelVisualDesignation(description);
                                        if (designation.IsValidDescription)
                                        {
                                            VoxelVisualDesignation master = designation.GetMasterVariant();
                                            if (!result.ContainsKey(master.Key))
                                                result.Add(master.Key, master);
                                        }
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
