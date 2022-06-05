using System.Collections.Generic;
using System;

public class VoxelVisualDesignation
{
    private readonly Designation[,,] description = new Designation[2, 2, 2];
    public Designation[,,] Description => description;

    public string Key { get { return ToString(); } }

    public bool IsValidDescription { get; }

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
        IsValidDescription = GetIsValid();
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

    private bool GetIsValid()
    {
        // A shell on the top side only exists if all bottom pieces are also shell
        if(GetTopDesignations().Any(item => item == Designation.Shell))
        {
            return GetBottomDesignations().All(item => item == Designation.Shell);
        }    
        return true;
    }

    private void CorrectValues()
    {
        // If has a platform designation on the top half, or is under a non-empty slot, set that designation to empty instead.
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if (description[x, 1, z] == Designation.Aquaduct
                    || description[x, 1, z] == Designation.Platform)
                {
                    description[x, 1, z] = Designation.Empty;
                }

                if (description[x, 0, z] == Designation.Aquaduct
                    || description[x, 0, z] == Designation.Platform)
                {
                    description[x, 1, z] = Designation.Empty;
                }
            }
        }

        // If a column or top half of a designation is filled, set those to SquaredWalkableRoof (the default building). 
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if (description[x, 1, z].IsBuilding)
                {
                    description[x, 1, z] = Designation.SquaredWalkableRoof;
                }

                if (description[x, 0, z].IsBuilding
                    && description[x, 1, z].IsBuilding)
                {
                    description[x, 0, z] = Designation.SquaredWalkableRoof;
                }
            }
        }

        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                if (description[x, 1, z].IsBuilding)
                {
                    description[x, 1, z] = Designation.SquaredWalkableRoof;
                }

                if (description[x, 0, z].IsBuilding
                    && description[x, 1, z].IsBuilding)
                {
                    description[x, 0, z] = Designation.SquaredWalkableRoof;
                }
            }
        }
        // A rounded Designation only exists on a convex or concave corner
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
