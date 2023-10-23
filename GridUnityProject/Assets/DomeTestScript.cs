using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelVisuals;

public class DomeTestScript : MonoBehaviour
{
    private SphereCollider sphere;
    [SerializeField]
    private CityBuildingMain main;

    public bool DoIt;
    private void Start()
    {
        sphere = GetComponent<SphereCollider>();
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
                item.Designation = Designation.Empty;
            }
            else
            {
                Vector3 spherePos = sphere.ClosestPoint(item.Position);
                bool isInside = !((spherePos - item.Position).magnitude > float.Epsilon);
                item.Designation = isInside ? Designation.Shell : Designation.Empty;
            }
        }
        main.InteractionMesh.RebuildMesh();
        main.UpdateAllVisuals();
    }
}
