public enum TopDesignations
{
    Empty,
    Shell,
    RoundedBuilding,
    SquaredBuilding,
}

public enum BottomDesignations
{
    Empty,
    Shell,
    RoundedWalkableRoof,
    SquaredWalkableRoof,
    RoundedSlantedRoof,
    SquaredSlantedRoof,
    Platform,
    Aquaduct
}
public abstract class VoxelDesignation
{
    public abstract string Key { get; }
    public abstract bool CanFillTopHalf { get; }
    public abstract bool CanFillBottomHalf { get; }
    public virtual bool IsEmpty => false;
    public virtual bool IsShell => false;
}

public class EmptyDesignation : VoxelDesignation
{
    public override string Key => "E";
    public override bool CanFillTopHalf => true;
    public override bool CanFillBottomHalf => true;
    public override bool IsEmpty => true;
}

// Shells cannot exist on top of components, but they can exist below and to the sides of components
public class ShellDesignation : VoxelDesignation
{
    public override string Key => "S";
    public override bool CanFillTopHalf => true;
    public override bool CanFillBottomHalf => true;
    public override bool IsShell => true;
}

public class AquaductDesignation : VoxelDesignation
{
    public override string Key => "A";
    public override bool CanFillTopHalf => false;
    public override bool CanFillBottomHalf => true;
}

public abstract class BuildingDesignation : VoxelDesignation
{
    public override bool CanFillTopHalf => true;
    public override bool CanFillBottomHalf => true;
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
    public override bool CanFillTopHalf => false;
    public override bool CanFillBottomHalf => true;
}

public class CoveredPlatformDesignation : PlatformDesignation
{
    public override string Key => "CP";
    public override bool CanFillTopHalf => true;
}
public class UncoveredPlatformDesignation : PlatformDesignation
{
    public override string Key => "UP";
}