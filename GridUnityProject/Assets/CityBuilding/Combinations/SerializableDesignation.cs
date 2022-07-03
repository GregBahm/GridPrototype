using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SerializableVisualDesignation
{
    [SerializeField]
    private SerializableDesignation X0Y0Z0;
    [SerializeField]
    private SerializableDesignation X0Y0Z1;
    [SerializeField]
    private SerializableDesignation X0Y1Z0;
    [SerializeField]
    private SerializableDesignation X0Y1Z1;
    [SerializeField]
    private SerializableDesignation X1Y0Z0;
    [SerializeField]
    private SerializableDesignation X1Y0Z1;
    [SerializeField]
    private SerializableDesignation X1Y1Z0;
    [SerializeField]
    private SerializableDesignation X1Y1Z1;

    public SerializableVisualDesignation(VoxelVisualDesignation source)
    {
        X0Y0Z0 = reversedTable[source.Description[0, 0, 0]];
        X0Y0Z1 = reversedTable[source.Description[0, 0, 1]];
        X0Y1Z0 = reversedTable[source.Description[0, 1, 0]];
        X0Y1Z1 = reversedTable[source.Description[0, 1, 1]];
        X1Y0Z0 = reversedTable[source.Description[1, 0, 0]];
        X1Y0Z1 = reversedTable[source.Description[1, 0, 1]];
        X1Y1Z0 = reversedTable[source.Description[1, 1, 0]];
        X1Y1Z1 = reversedTable[source.Description[1, 1, 1]];
    }

    public VoxelVisualDesignation ToDesignation()
    {
        Designation[] designation = new Designation[]
        {
            conversionTable[X0Y0Z0],
            conversionTable[X0Y0Z1],
            conversionTable[X0Y1Z0],
            conversionTable[X0Y1Z1],
            conversionTable[X1Y0Z0],
            conversionTable[X1Y0Z1],
            conversionTable[X1Y1Z0],
            conversionTable[X1Y1Z1],

        };
        return new VoxelVisualDesignation(designation);
    }

    public enum SerializableDesignation
    {
        Empty = 0,
        Shell = 1,
        SquaredWalkableRoof = 2,
        SquaredSlantedRoof = 3,
        Platform = 4,
        Aquaduct = 5
    }

    private static Dictionary<SerializableDesignation, Designation> conversionTable = new Dictionary<SerializableDesignation, Designation>()
    { 
        {SerializableDesignation.Empty, Designation.Empty },
        {SerializableDesignation.Shell, Designation.Shell },
        {SerializableDesignation.SquaredWalkableRoof, Designation.SquaredSlantedRoof },
        {SerializableDesignation.SquaredSlantedRoof, Designation.SquaredWalkableRoof },
        {SerializableDesignation.Platform, Designation.Platform },
        {SerializableDesignation.Aquaduct, Designation.Aquaduct },

    };

    private static Dictionary<Designation, SerializableDesignation> reversedTable;

    static SerializableVisualDesignation()
    {
        reversedTable = conversionTable.ToDictionary(item => item.Value, item => item.Key);
    }
}
