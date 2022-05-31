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
        CornerCombination[] topCombinations = GetTopCombinations().ToArray();
        CornerCombination[] bottomCombinations = GetBottomCombinations().ToArray();
        int total = topCombinations.Length + bottomCombinations.Length;
        Console.WriteLine(total);
    }

    private IEnumerable<CornerCombination> GetTopCombinations()
    {
        Dictionary<string, CornerCombination> result = new Dictionary<string, CornerCombination>();
        foreach (Designation core in Designation.TopCores.Where(item => item != Designation.Empty))
        {
            foreach (Designation x in Designation.TopHorizontalNeighbors)
            {
                foreach (Designation z in Designation.TopHorizontalNeighbors)
                {
                    IEnumerable<Designation> topYConnections = Designation.TopYConnections;
                    if (core == Designation.Shell)
                        topYConnections = new Designation[] { Designation.Shell };
                    foreach (Designation y in topYConnections)
                    {
                        CornerCombination combo = new CornerCombination(core, y, x, z);
                        CornerCombination mirror = combo.GetMirrored();
                        if (!result.ContainsKey(combo.Key) && !result.ContainsKey(mirror.Key))
                            result.Add(combo.Key, combo);
                    }
                }
            }
        }
        return result.Values;
    }

    private IEnumerable<CornerCombination> GetBottomCombinations()
    {
        throw new NotImplementedException();
    }
}

public class CornerCombination
{
    public Designation Core { get; }
    public Designation XConnection { get; }
    public Designation ZConnection { get; }
    public Designation YConnection { get; }
    public string Key { get; }

    public CornerCombination(Designation core, Designation xConnection, Designation yConnection, Designation zConnection)
    {
        Core = core;
        XConnection = xConnection;
        YConnection = yConnection;
        ZConnection = zConnection;
        Key = core.Name + " " + XConnection.Name + " " + YConnection.Name + " " + ZConnection.Name;
    }

    public CornerCombination GetMirrored()
    {
        return new CornerCombination(Core, ZConnection, YConnection, XConnection);
    }
}