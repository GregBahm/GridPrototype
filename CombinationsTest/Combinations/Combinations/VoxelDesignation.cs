public abstract class VoxelDesignation
{
    public abstract string Key { get; }
    public abstract bool CanFillTopHalf { get; }
}

public class EmptyDesignation : VoxelDesignation
{
    public override string Key => "E";
    public override bool CanFillTopHalf => true;
}

public class ShellDesignation : VoxelDesignation
{
    public override string Key => "S";
    public override bool CanFillTopHalf => true;
}

public class AquaductDesignation : VoxelDesignation
{
    public override string Key => "A";
    public override bool CanFillTopHalf => false;
}

public abstract class BuildingDesignation : VoxelDesignation
{
    public override bool CanFillTopHalf => true;
}

public class CorneredWalkableRoofDesignation : BuildingDesignation
{
    public override string Key => "CW";
}

public class CorneredSlantedRoofDesignation : BuildingDesignation
{
    public override string Key => "CS";
}

public class RoundedWalkableRoofDesignation : BuildingDesignation
{
    public override string Key => "RW";
}

public class RoundedSlantedRoofDesignation : BuildingDesignation
{
    public override string Key => "RS";
}

public abstract class PlatformDesignation :VoxelDesignation
{
    public override bool CanFillTopHalf => false
}

public class CoveredPlatformDesignation : PlatformDesignation
{
    public override string Key => "CP";
}
public class UncoveredPlatformDesignation : PlatformDesignation
{
    public override string Key => "UP";
}