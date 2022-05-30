public abstract class VoxelDesignation
{
}

public class EmptyDesignation : VoxelDesignation
{
    public override string ToString()
    {
        return "Empty";
    }
}

public class ShellDesignation : VoxelDesignation { }

public class AquaductDesignation : VoxelDesignation { }

public class BuildingDesignation : VoxelDesignation
{
    public DesignationBuildingRoof Roof { get; }
    public DesignationBuildingWall Wall { get; }

    public BuildingDesignation(DesignationBuildingRoof roof, DesignationBuildingWall wall)
    {
        Roof = roof;
        Wall = wall;
    }
}

public class PlatformDesignation : VoxelDesignation
{
    public DesignationPlatformType Type { get; }

    public PlatformDesignation(DesignationPlatformType type)
    {
        Type = type;
    }
}

public enum DesignationPlatformType
{
    Uncovered,
    Grass,
    Covered,
}

public enum DesignationBuildingRoof
{
    Grass,
    Stone,
    Slanted,
}

public enum DesignationBuildingWall
{
    Cornered,
    Rounded,
}