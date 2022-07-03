
using System;
using System.Collections.Generic;

public class Designation
{
    public string Name { get; }
    public bool IsBuilding { get; }

    public Designation(string name, bool isBuilding = false)
    {
        Name = name;
        IsBuilding = isBuilding;
    }

    public override string ToString()
    {
        return Name;
    }

    public static Designation Empty { get; } = new Designation("Empty");
    public static Designation Shell { get; } = new Designation("Shell");
    //public static Designation RoundedWalkableRoof { get; } = new Designation("RoundedWalkableRoof", true);
    public static Designation SquaredWalkableRoof { get; } = new Designation("SquaredWalkableRoof", true);
    //public static Designation RoundedSlantedRoof { get; } = new Designation("RoundedSlantedRoof", true);
    public static Designation SquaredSlantedRoof { get; } = new Designation("SquaredSlantedRoof", true);
    public static Designation Platform { get; } = new Designation("Platform");
    public static Designation Aquaduct { get; } = new Designation("Aquaduct");
    public static IEnumerable<Designation> AllBaseDesignations { get; }

    static Designation()
    {
        AllBaseDesignations = new Designation[]
        {
            Empty,
            Shell,
            SquaredWalkableRoof,
            SquaredSlantedRoof,
            //RoundedSlantedRoof, // Someday...
            //RoundedWalkableRoof,
            Platform,
            Aquaduct,
        };
    }
}