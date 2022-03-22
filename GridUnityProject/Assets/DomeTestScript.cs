using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelVisuals;

public class DomeTestScript : MonoBehaviour
{
    public SphereCollider Sphere;

    public bool DoIt;
    private CityBuildingMain main;
    private void Start()
    {
        main = GetComponent<CityBuildingMain>();
    }

    private void Update()
    {
        if(DoIt)
        {
            //DoIt = false;
            DoTheThing();
        }
    }

    private void DoTheThing()
    {
        foreach (DesignationCell item in main.MainGrid.DesignationCells)
        {
            if(item.GroundPoint.IsBorder)
            {
                item.Designation = VoxelDesignationType.Empty;
            }
            else
            {
                Vector3 spherePos = Sphere.ClosestPoint(item.Position);
                bool isInside = !((spherePos - item.Position).magnitude > float.Epsilon);
                item.Designation = isInside ? VoxelDesignationType.WalkableRoof : VoxelDesignationType.Empty;
            }
        }
        main.UpdateInteractionGrid();
        main.UpdateAllVisuals();
    }
}
