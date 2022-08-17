using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SerializableVisualDesignation
{
    [SerializeField]
    private Designation X0Y0Z0;
    [SerializeField]
    private Designation X0Y0Z1;
    [SerializeField]
    private Designation X0Y1Z0;
    [SerializeField]
    private Designation X0Y1Z1;
    [SerializeField]
    private Designation X1Y0Z0;
    [SerializeField]
    private Designation X1Y0Z1;
    [SerializeField]
    private Designation X1Y1Z0;
    [SerializeField]
    private Designation X1Y1Z1;

    public SerializableVisualDesignation(VoxelVisualDesignation source)
    {
        X0Y0Z0 = source.Description[0, 0, 0];
        X0Y0Z1 = source.Description[0, 0, 1];
        X0Y1Z0 = source.Description[0, 1, 0];
        X0Y1Z1 = source.Description[0, 1, 1];
        X1Y0Z0 = source.Description[1, 0, 0];
        X1Y0Z1 = source.Description[1, 0, 1];
        X1Y1Z0 = source.Description[1, 1, 0];
        X1Y1Z1 = source.Description[1, 1, 1];
    }

    public VoxelVisualDesignation ToDesignation()
    {
        Designation[] designation = new Designation[]
        {
            X0Y0Z0,
            X0Y0Z1,
            X0Y1Z0,
            X0Y1Z1,
            X1Y0Z0,
            X1Y0Z1,
            X1Y1Z0,
            X1Y1Z1,

        };
        return new VoxelVisualDesignation(designation);
    }
}
