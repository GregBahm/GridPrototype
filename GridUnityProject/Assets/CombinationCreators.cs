using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using System;

public class CombinationCreators : MonoBehaviour
{
    private List<VoxelBlueprint> baseBlueprints;
    private List<VoxelBlueprint> invariants;

    public string[] baseNames;
    public string[] invariantNames;

    private void Start()
    {
        baseBlueprints = GetAllUniqueCombinations().ToList();
        invariants = GetInvariants().ToList();
        VerifyInvariants();

        baseNames = baseBlueprints.Select(item => item.ToString()).ToArray();
        invariantNames = invariants.Select(item => item.ToString()).ToArray();
    }

    private void VerifyInvariants()
    {
        HashSet<string> invariantsTest = new HashSet<string>();
        foreach (VoxelBlueprint item in invariants)
        {
            if (!invariantsTest.Add(item.ToString()))
            {
                throw new Exception("Not Unique");
            }
            List<VoxelBlueprint> variants = item.GetUniqueVariants().ToList();
            foreach (VoxelBlueprint variant in variants)
            {
                if (!invariantsTest.Add(variant.ToString()))
                {
                    throw new Exception("Not Unique");
                }
            }
        }
        if(invariantsTest.Count != 256)
        {
            throw new Exception("Doesn't sum up to the right amout");
        }
    }

    public IEnumerable<VoxelBlueprint> GetInvariants()
    {
        Dictionary<string, VoxelBlueprint> invariantsDictionary = baseBlueprints.ToDictionary(item => item.ToString(), item => item);
        foreach (VoxelBlueprint item in baseBlueprints)
        {
            if(invariantsDictionary.ContainsKey(item.ToString()))
            {
                List<VoxelBlueprint> variants = item.GetUniqueVariants().ToList();
                foreach (VoxelBlueprint variant in variants)
                {
                    if (invariantsDictionary.ContainsKey(variant.ToString()))
                    {
                        invariantsDictionary.Remove(variant.ToString());
                    }
                }
            }
        }
        return invariantsDictionary.Values;
    }

    public static IEnumerable<VoxelBlueprint> GetAllUniqueCombinations() // I know. Silly. But it's one-use code and I don't want to think.
    {
        bool aVal = false;
        bool bVal = false;
        bool cVal = false;
        bool dVal = false;
        bool eVal = false;
        bool fVal = false;
        bool gVal = false;
        bool hVal = false;

        for (int a = 0; a < 2; a++)
        {
            for (int b = 0; b < 2; b++)
            {
                for (int c = 0; c < 2; c++)
                {
                    for (int d = 0; d < 2; d++)
                    {
                        for (int e = 0; e < 2; e++)
                        {
                            for (int f = 0; f < 2; f++)
                            {
                                for (int g = 0; g < 2; g++)
                                {
                                    for (int h = 0; h < 2; h++)
                                    {

                                        bool[] vals = new bool[] { aVal, bVal, cVal, dVal, eVal, fVal, gVal, hVal };
                                        yield return new VoxelBlueprint(vals);
                                        hVal = !hVal;
                                    }
                                    gVal = !gVal;
                                }
                                fVal = !fVal;
                            }
                            eVal = !eVal;
                        }
                        dVal = !dVal;
                    }
                    cVal = !cVal;
                }
                bVal = !bVal;
            }
            aVal = !aVal;
        }
    }
}

[Serializable]
public class VoxelBlueprint
{
    public bool[,,] Description { get; } = new bool[2, 2, 2];

    public VoxelBlueprint()
    {
    }

    public override string ToString()
    {
        string ret = "";
        foreach (bool item in Description)
        {
            ret += item.ToString() + " ";
        }
        return ret;
    }

    public VoxelBlueprint(bool[] values)
    {
        Description[0, 1, 1] = values[0];
        Description[1, 1, 1] = values[1];
        Description[0, 0, 1] = values[2];
        Description[1, 0, 1] = values[3];
        Description[0, 1, 0] = values[4];
        Description[1, 1, 0] = values[5];
        Description[0, 0, 0] = values[6];
        Description[1, 0, 0] = values[7];
    }

    public bool Matches(VoxelBlueprint blueprint)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    if (Description[x, y, z] != blueprint.Description[x, y, z])
                        return false;
                }
            }
        }
        return true;
    }

    public VoxelBlueprint GetFlipped()
    {
        VoxelBlueprint ret = new VoxelBlueprint();
        for (int y = 0; y < 2; y++)
        {
            for (int z = 0; z < 2; z++)
            {
                bool left = Description[0, y, z];
                bool right = Description[1, y, z];
                ret.Description[0, y, z] = right;
                ret.Description[1, y, z] = left;
            }
        }
        return ret;
    }

    public VoxelBlueprint GetRotated()
    {
        VoxelBlueprint ret = new VoxelBlueprint();
        for (int y = 0; y < 2; y++)
        {
            bool one = Description[0, y, 0];
            bool two = Description[1, y, 0];
            bool three = Description[1, y, 1];
            bool four = Description[0, y, 1];

            ret.Description[0, y, 0] = two;
            ret.Description[1, y, 0] = three;
            ret.Description[1, y, 1] = four;
            ret.Description[0, y, 1] = one;
        }
        return ret;
    }

    public IEnumerable<VoxelBlueprint> GetUniqueVariants()
    {
        VoxelBlueprint rotated = GetRotated();
        VoxelBlueprint rotatedTwice = rotated.GetRotated();

        VoxelBlueprint flipped = GetFlipped();
        VoxelBlueprint flippedRotated = flipped.GetRotated();
        VoxelBlueprint flippedRotatedTwice = flippedRotated.GetRotated();

        VoxelBlueprint[] rawVariants = new VoxelBlueprint[]
        {
            rotated,
            rotatedTwice,
            rotatedTwice.GetRotated(),

            flipped,
            flippedRotated,
            flippedRotatedTwice,
            flippedRotatedTwice.GetRotated()
        };

        HashSet<string> uniquenessCheck = new HashSet<string>
        {
            ToString()
        };
        foreach (VoxelBlueprint rawVariant in rawVariants)
        {
            if(uniquenessCheck.Add(rawVariant.ToString()))
            {
                yield return rawVariant;
            }
        }
    }
}