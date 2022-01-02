using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class VoxelDesignation
{
    [SerializeField]
    private readonly SlotType[,,] description = new SlotType[2, 2, 2];
    public SlotType[,,] Description => description;

    public string Key { get { return ToString(); } }

    public VoxelDesignation()
    {}

    public VoxelDesignation(SlotType[] values)
    {
        Description[0, 0, 0] = values[0];
        Description[0, 0, 1] = values[1];
        Description[0, 1, 0] = values[2];
        Description[0, 1, 1] = values[3];
        Description[1, 0, 0] = values[4];
        Description[1, 0, 1] = values[5];
        Description[1, 1, 0] = values[6];
        Description[1, 1, 1] = values[7];
    }
    public static string GetDesignationKey(SlotType[,,] description)
    {
        return "X0Y0Z0: " + description[0, 0, 0].ToString() + "\n" +
               "X0Y0Z1: " + description[0, 0, 1].ToString() + "\n" +
               "X0Y1Z0: " + description[0, 1, 0].ToString() + "\n" +
               "X0Y1Z1: " + description[0, 1, 1].ToString() + "\n" +
               "X1Y0Z0: " + description[1, 0, 0].ToString() + "\n" +
               "X1Y0Z1: " + description[1, 0, 1].ToString() + "\n" +
               "X1Y1Z0: " + description[1, 1, 0].ToString() + "\n" +
               "X1Y1Z1: " + description[1, 1, 1].ToString();
    }

    public override string ToString()
    {
        return GetDesignationKey(this.description);
    }

    public GeneratedVoxelDesignation GetFlipped()
    {
        GeneratedVoxelDesignation ret = new GeneratedVoxelDesignation(true, 0);
        for (int y = 0; y < 2; y++)
        {
            for (int z = 0; z < 2; z++)
            {
                SlotType left = Description[0, y, z];
                SlotType right = Description[1, y, z];
                ret.Description[0, y, z] = right;
                ret.Description[1, y, z] = left;
            }
        }
        return ret;
    }

    public GeneratedVoxelDesignation GetRotated(int rotationCount, bool wasFlipped)
    {
        GeneratedVoxelDesignation ret = new GeneratedVoxelDesignation(wasFlipped, rotationCount);
        for (int y = 0; y < 2; y++)
        {
            SlotType one = Description[0, y, 0];
            SlotType two = Description[1, y, 0];
            SlotType three = Description[1, y, 1];
            SlotType four = Description[0, y, 1];

            ret.Description[0, y, 0] = two;
            ret.Description[1, y, 0] = three;
            ret.Description[1, y, 1] = four;
            ret.Description[0, y, 1] = one;
        }
        return ret;
    }

    public IEnumerable<GeneratedVoxelDesignation> GetUniqueVariants()
    {
        GeneratedVoxelDesignation rotated = GetRotated(1, false);
        GeneratedVoxelDesignation rotatedTwice = rotated.GetRotated(2, false);

        GeneratedVoxelDesignation flipped = GetFlipped();
        GeneratedVoxelDesignation flippedRotated = flipped.GetRotated(1, true);
        GeneratedVoxelDesignation flippedRotatedTwice = flippedRotated.GetRotated(2, true);

        GeneratedVoxelDesignation[] rawVariants = new GeneratedVoxelDesignation[]
        {
            rotated,
            rotatedTwice,
            rotatedTwice.GetRotated(3, false),

            flipped,
            flippedRotated,
            flippedRotatedTwice,
            flippedRotatedTwice.GetRotated(3, true)
        };

        HashSet<string> uniquenessCheck = new HashSet<string>
        {
            ToString()
        };
        foreach (GeneratedVoxelDesignation rawVariant in rawVariants)
        {
            if(uniquenessCheck.Add(rawVariant.ToString()))
            {
                yield return rawVariant;
            }
        }
    }
}
