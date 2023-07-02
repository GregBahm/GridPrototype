using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using JetBrains.Annotations;

public class VoxelVisualDesignation
{
    private Designation[,,] description = new Designation[2, 2, 2];
    public Designation[,,] Description => description;

    public IEnumerable<Designation> FlatDescription
    {
        get
        {
            yield return X0Y0Z0;
            yield return X0Y0Z1;
            yield return X0Y1Z0;
            yield return X0Y1Z1;
            yield return X1Y0Z0;
            yield return X1Y0Z1;
            yield return X1Y1Z0;
            yield return X1Y1Z1;
        }
    }

    public string Key { get { return ToString(); } }

    private static HashSet<Designation> isBuildingTable = new HashSet<Designation>() { Designation.SquaredWalkableRoof, Designation.SquaredSlantedRoof };

    public Designation X0Y0Z0 => description[0, 0, 0];
    public Designation X0Y0Z1 => description[0, 0, 1];
    public Designation X0Y1Z0 => description[0, 1, 0];
    public Designation X0Y1Z1 => description[0, 1, 1];
    public Designation X1Y0Z0 => description[1, 0, 0];
    public Designation X1Y0Z1 => description[1, 0, 1];
    public Designation X1Y1Z0 => description[1, 1, 0];
    public Designation X1Y1Z1 => description[1, 1, 1];

    public VoxelVisualDesignation(Designation[] values)
    {
        Description[0, 0, 0] = values[0];
        Description[0, 0, 1] = values[1];
        Description[0, 1, 0] = values[2];
        Description[0, 1, 1] = values[3];
        Description[1, 0, 0] = values[4];
        Description[1, 0, 1] = values[5];
        Description[1, 1, 0] = values[6];
        Description[1, 1, 1] = values[7];

        CorrectValues();
    }

    private IEnumerable<Designation> GetTopDesignations()
    {
        yield return Description[0, 1, 0];
        yield return Description[0, 1, 1];
        yield return Description[1, 1, 0];
        yield return Description[1, 1, 1];
    }
    private IEnumerable<Designation> GetBottomDesignations()
    {
        yield return Description[0, 0, 0];
        yield return Description[0, 0, 1];
        yield return Description[1, 0, 0];
        yield return Description[1, 0, 1];
    }

    protected VoxelVisualDesignation() { }

    private bool IsBuilding(Designation designation)
    {
        return isBuildingTable.Contains(designation);
    }

    private void CorrectValues()
    {
        CorrectEnclosedDesignations();
        CorrectRoofDesignations();
    }

    // if a designation is enclosed on all sides (nothing open) set it to SquaredWalkableRoof (the default building). 
    private void CorrectEnclosedDesignations()
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    IEnumerable<Designation> adjacentDesignations = GetAdjacentDesignations(x, y, z);
                    if (description[x, y, z] != Designation.Empty 
                        && description[x, y, z] != Designation.Shell
                        && adjacentDesignations.All(item => item != Designation.Empty))
                    {
                        description[x, y, z] = Designation.SquaredWalkableRoof;
                    }
                }
            }
        }
    }

    private IEnumerable<Designation> GetAdjacentDesignations(int x, int y, int z)
    {
        int adjacentX = x == 0 ? 1 : 0;
        int adjacentY = y == 0 ? 1 : 0;
        int adjacentZ = z == 0 ? 1 : 0;
        yield return Description[adjacentX, y, z];
        yield return Description[x, adjacentY, z];
        yield return Description[x, y, adjacentZ];
    }

    // If a column or top half of a designation is slanted roof, set those to SquaredWalkableRoof (the default building). 
    private void CorrectRoofDesignations()
    {
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if (IsBuilding(description[x, 1, z]))
                {
                    description[x, 1, z] = Designation.SquaredWalkableRoof;
                }

                if (IsBuilding(description[x, 0, z])
                    && IsBuilding(description[x, 1, z]))
                {
                    description[x, 0, z] = Designation.SquaredWalkableRoof;
                }
            }
        }
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if (IsBuilding(description[x, 1, z]))
                {
                    description[x, 1, z] = Designation.SquaredWalkableRoof;
                }

                if (IsBuilding(description[x, 0, z])
                    && IsBuilding(description[x, 1, z]))
                {
                    description[x, 0, z] = Designation.SquaredWalkableRoof;
                }
            }
        }
    }

    public bool GetIsValid()
    {
        return VoxelVisualDesignationValidator.IsValid(this);
    }

    public static string GetDesignationKey(Designation[,,] description)
    {
        return description[0, 0, 0].ToString() + " " +
               description[0, 0, 1].ToString() + " " +
               description[0, 1, 0].ToString() + " " +
               description[0, 1, 1].ToString() + " " +
               description[1, 0, 0].ToString() + " " +
               description[1, 0, 1].ToString() + " " +
               description[1, 1, 0].ToString() + " " +
               description[1, 1, 1].ToString();
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
                Designation left = Description[0, y, z];
                Designation right = Description[1, y, z];
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
            Designation one = Description[0, y, 0];
            Designation two = Description[1, y, 0];
            Designation three = Description[1, y, 1];
            Designation four = Description[0, y, 1];

            ret.Description[0, y, 0] = two;
            ret.Description[1, y, 0] = three;
            ret.Description[1, y, 1] = four;
            ret.Description[0, y, 1] = one;
        }
        return ret;
    }

    public VoxelVisualDesignation GetMasterVariant()
    {
        IEnumerable<VoxelVisualDesignation> set = new VoxelVisualDesignation[] { this };
        return set.Concat(GetUniqueVariants()).OrderBy(item => item.Key).First();
    }

    public IEnumerable<GeneratedVoxelDesignation> GetUniqueVariants(bool includeOriginal = false)
    {
        GeneratedVoxelDesignation rotated = GetRotated(1, false);
        GeneratedVoxelDesignation rotatedTwice = rotated.GetRotated(2, false);
        GeneratedVoxelDesignation rotatedThrice = rotatedTwice.GetRotated(3, false);

        GeneratedVoxelDesignation flipped = GetFlipped();

        GeneratedVoxelDesignation flippedRotated = flipped.GetRotated(1, true);
        GeneratedVoxelDesignation flippedRotatedTwice = flippedRotated.GetRotated(2, true);
        GeneratedVoxelDesignation flippedRotatedThrice = flippedRotatedTwice.GetRotated(3, true);

        List<GeneratedVoxelDesignation> rawVariants = new List<GeneratedVoxelDesignation>
        {
            rotated,
            rotatedTwice,
            rotatedThrice,

            flipped,

            flippedRotated,
            flippedRotatedTwice,
            flippedRotatedThrice
        };
        if (includeOriginal)
        {
            GeneratedVoxelDesignation original = new GeneratedVoxelDesignation(false, 0);
            original.description = Description;
            rawVariants.Add(original);
            yield return original;
        }

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

// Determines if a voxel visual designation is valid
public static class VoxelVisualDesignationValidator
{
    public static bool IsValid(VoxelVisualDesignation designtion)
    {
        bool shellAboveNonShell = IsAnyShellAboveNonshell(designtion.Description);
        bool bottomShellsDiagonal = AreShellDesignationsDiagonal(designtion.Description, 0);
        bool topShellsDiagonal = AreShellDesignationsDiagonal(designtion.Description, 1);
        return !shellAboveNonShell && !bottomShellsDiagonal && !topShellsDiagonal;
    }

    // A shell on the top side is only valid if above a shell
    private static bool IsAnyShellAboveNonshell(Designation[,,] description)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if (description[x, 1, z] == Designation.Shell
                    && description[x, 0, z] != Designation.Shell)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // A shell designation can't be diagonal to another shell designation unless the whole set is shell
    private static bool AreShellDesignationsDiagonal(Designation[,,] description, int height)
    {
        int shellCount = 0;
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if (description[x, height, z] == Designation.Shell)
                {
                    shellCount++;
                }
            }
        }
        if(shellCount == 2)
        {
            if (description[0, height, 0] == Designation.Shell
                && description[1, height, 1] == Designation.Shell)
            {
                return true;
            }
            if (description[1, height, 0] == Designation.Shell
                    && description[0, height, 1] == Designation.Shell)
            {
                return true;
            }
        }
        return false;
    }
}
