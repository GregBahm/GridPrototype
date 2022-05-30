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
        //VoxelVisualDesignation[] uniqueCombinations = GetAllUniqueCombinations().ToArray();
        CornerCombination[] cornerCombinations = GetAllCornerCombinations().ToArray();
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

    private IEnumerable<VoxelDesignation> GetBottomComponents()
    {
        return  GetUniqueComponents().Where(item => item.CanFillTopHalf).ToArray();
    }

    private IEnumerable<VoxelDesignation> GetTopComponents()
    {
        return GetUniqueComponents().Where(item => item.CanFillBottomHalf).ToArray();
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

    private IEnumerable<CornerCombination> GetAllCornerCombinations()
    {
        VoxelDesignation[] topComponents = GetTopComponents().ToArray();
        VoxelDesignation[] bottomComponents = GetBottomComponents().ToArray();
        Dictionary<string, CornerCombination> result = new Dictionary<string, CornerCombination>();
        long counter = 0;
        foreach(VoxelDesignation corner in topComponents.Where(item => !item.IsEmpty))
        {
            foreach (VoxelDesignation y in bottomComponents)
            {
                foreach (VoxelDesignation x in topComponents)
                {
                    foreach(VoxelDesignation z in topComponents)
                    {
                        CornerCombination combo = new CornerCombination(corner, y, x, z, true);
                        CornerCombination mirror = combo.GetMirrored();
                        if (!result.ContainsKey(combo.Key) && !result.ContainsKey(mirror.Key))
                            result.Add(combo.Key, combo);
                        counter++;

                    }
                }
            }
        }
        foreach (VoxelDesignation corner in bottomComponents)
        {
            foreach (VoxelDesignation y in topComponents.Where(item => !item.IsEmpty && !item.IsShell))
            {
                foreach (VoxelDesignation x in bottomComponents)
                {
                    foreach (VoxelDesignation z in bottomComponents)
                    {
                        CornerCombination combo = new CornerCombination(corner, y, x, z, true);
                        CornerCombination mirror = combo.GetMirrored();
                        if (!result.ContainsKey(combo.Key) && !result.ContainsKey(mirror.Key))
                            result.Add(combo.Key, combo);
                        counter++;
                    }
                }
            }
        }
        return result.Values;
    }
}
class CornerCombination
{
    public VoxelDesignation Corner { get; }
    public VoxelDesignation YConnection { get; }
    public VoxelDesignation XConnection { get; }
    public VoxelDesignation ZConnection { get; }

    public bool IsTop { get; }

    public string Key { get; }

    public CornerCombination(VoxelDesignation corner, 
        VoxelDesignation yConnection, 
        VoxelDesignation xConnection, 
        VoxelDesignation zConnection, 
        bool isTop)
    {
        Corner = corner;
        YConnection = yConnection;
        XConnection = xConnection;
        ZConnection = zConnection;
        IsTop = isTop;
        Key = Corner.Key + " "
            + YConnection.Key + " "
            + XConnection.Key + " "
            + ZConnection.Key + " "
            + IsTop.ToString();
    }

    public CornerCombination GetMirrored()
    {
        return new CornerCombination(Corner,
            YConnection,
            ZConnection,
            XConnection,
            IsTop);
    }
}