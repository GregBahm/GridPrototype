using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using System;

public class CombinationCreators : MonoBehaviour
{
    private List<VoxelDesignation> baseBlueprints;
    private List<VoxelDesignation> invariants;

    public GameObject BlueprintDisplayPrefab;

    private void Start()
    {
        baseBlueprints = GetAllUniqueCombinations().ToList();
        invariants = GetInvariants().ToList();
        VerifyInvariants();
        CreateDisplayPrefabs();
    }

    private void CreateDisplayPrefabs()
    {
        int index = 0;
        int rootCount = Mathf.CeilToInt(Mathf.Sqrt(invariants.Count));
        foreach (VoxelDesignation item in invariants)
        {
            GameObject obj = Instantiate(BlueprintDisplayPrefab);
            VoxelBlueprintDisplay behavior = obj.GetComponent<VoxelBlueprintDisplay>();
            behavior.SetBlueprint(item);
            int x = Mathf.FloorToInt(index / rootCount);
            int y = index % rootCount;
            obj.transform.position = new Vector3(x * 5, 0, y * 5);
            obj.name = item.ToString();
            index++;
        }
    }

    private void VerifyInvariants()
    {
        HashSet<string> invariantsTest = new HashSet<string>();
        foreach (VoxelDesignation item in invariants)
        {
            if (!invariantsTest.Add(item.ToString()))
            {
                throw new Exception("Not Unique");
            }
            List<VoxelDesignation> variants = item.GetUniqueVariants().ToList();
            foreach (VoxelDesignation variant in variants)
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

    public IEnumerable<VoxelDesignation> GetInvariants()
    {
        Dictionary<string, VoxelDesignation> invariantsDictionary = baseBlueprints.ToDictionary(item => item.ToString(), item => item);
        foreach (VoxelDesignation item in baseBlueprints)
        {
            if(invariantsDictionary.ContainsKey(item.ToString()))
            {
                List<VoxelDesignation> variants = item.GetUniqueVariants().ToList();
                foreach (VoxelDesignation variant in variants)
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

    public static IEnumerable<VoxelDesignation> GetAllUniqueCombinations() // I know. Silly. But it's one-use code and I don't want to think.
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
                                        yield return new VoxelDesignation(vals);
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

[CreateAssetMenu(menuName = "VoxelDefinition/Voxel")]
public class VoxelDefinition : ScriptableObject
{
    public Mesh Art { get; }
    public VoxelDesignation Designation;
}

[CreateAssetMenu(menuName = "VoxelDefinition/Connection")]
public class VoxelConnection : ScriptableObject
{

}

[CreateAssetMenu(menuName = "VoxelDefinition/Designation")]
public class VoxelDesignation : ScriptableObject
{
    public bool[,,] Description { get; } = new bool[2, 2, 2];

    public VoxelDesignation()
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

    public VoxelDesignation(bool[] values)
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

    public VoxelDesignation GetFlipped()
    {
        VoxelDesignation ret = new VoxelDesignation();
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

    public VoxelDesignation GetRotated()
    {
        VoxelDesignation ret = new VoxelDesignation();
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

    public IEnumerable<VoxelDesignation> GetUniqueVariants()
    {
        VoxelDesignation rotated = GetRotated();
        VoxelDesignation rotatedTwice = rotated.GetRotated();

        VoxelDesignation flipped = GetFlipped();
        VoxelDesignation flippedRotated = flipped.GetRotated();
        VoxelDesignation flippedRotatedTwice = flippedRotated.GetRotated();

        VoxelDesignation[] rawVariants = new VoxelDesignation[]
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
        foreach (VoxelDesignation rawVariant in rawVariants)
        {
            if(uniquenessCheck.Add(rawVariant.ToString()))
            {
                yield return rawVariant;
            }
        }
    }
}
