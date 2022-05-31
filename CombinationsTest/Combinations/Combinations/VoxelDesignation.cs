
public class Designation
{
    public string Name { get; }
    public Designation(string name)
    {
        Name = name;
    }

    public static Designation Empty { get; } = new Designation("Empty");
    public static Designation Shell { get; } = new Designation("Shell");
    public static Designation RoundedBuilding { get; } = new Designation("RoundedBuilding");
    public static Designation SquaredBuilding { get; } = new Designation("SquaredBuilding");
    public static Designation RoundedWalkableRoof { get; } = new Designation("RoundedWalkableRoof");
    public static Designation SquaredWalkableRoof { get; } = new Designation("SquaredWalkableRoof");
    public static Designation RoundedSlantedRoof { get; } = new Designation("RoundedSlantedRoof");
    public static Designation SquaredSlantedRoof { get; } = new Designation("SquaredSlantedRoof");
    public static Designation Platform { get; } = new Designation("Platform");
    public static Designation Aquaduct { get; } = new Designation("Aquaduct");
    public static IEnumerable<Designation> TopCores { get; }
    public static IEnumerable<Designation> TopHorizontalNeighbors { get; }
    public static IEnumerable<Designation> TopYConnections { get; }

    public static IEnumerable<Designation> BottomHorizontalNeighbors { get; }
    static Designation()
    {
        TopCores = new Designation[]
        {
            Empty,
            Shell,
            RoundedBuilding,
            SquaredBuilding
        };

        TopHorizontalNeighbors = new Designation[]
        {
            Empty,
            Shell,
            RoundedBuilding,
            SquaredBuilding
        };

        TopYConnections = new Designation[]
        {
            Empty,
            Shell,
            RoundedBuilding,
            SquaredBuilding
        };

        BottomHorizontalNeighbors = new Designation[]
        {
            Empty,
            Shell,
            RoundedWalkableRoof, // Only exists if top designation is empty
            SquaredWalkableRoof, // Only exists if top designation is empty
            RoundedSlantedRoof, // Only exists if top designation is empty
            SquaredSlantedRoof, // Only exists if top designation is empty
            Platform,
            Aquaduct,
        };
    }
}