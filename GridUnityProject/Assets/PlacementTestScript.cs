using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementTestScript : MonoBehaviour
{
    private Transform transformHelper;
    public Transform Box;
    public Transform PointA;
    public Transform PointB;
    public Transform PointC;
    public Transform PointD;

    private void Start()
    {
        transformHelper = new GameObject().transform;
    }

    private void Update()
    {
        Vector3[] anchors = new Vector3[] {
            PointA.position,
            PointB.position,
            PointC.position,
            PointD.position,
        };
        SetBoxTransform(Box, anchors);
    }

    private void SetBoxTransform(Transform box, Vector3[] anchors)
    {
        PlacementHelper helper = new PlacementHelper(anchors);
        transformHelper.forward = helper.RotationVector;
        Vector3 localA = transformHelper.worldToLocalMatrix.MultiplyPoint(PointA.position);
        Vector3 localB = transformHelper.worldToLocalMatrix.MultiplyPoint(PointB.position);
        Vector3 localC = transformHelper.worldToLocalMatrix.MultiplyPoint(PointC.position);
        Vector3 localD = transformHelper.worldToLocalMatrix.MultiplyPoint(PointD.position);
        Vector3[] locals = new Vector3[] { localA, localB, localC, localD };
        float maxX = locals.Max(item => item.x);
        float minX = locals.Min(item => item.x);
        float maxZ = locals.Max(item => item.z);
        float minZ = locals.Min(item => item.z);

        Vector3 center = new Vector3((maxX + minX) / 2, -1f, (maxZ + minZ) / 2);
        Vector3 localScale = new Vector3(maxX - minX, 1, maxZ - minZ);

        box.SetParent(transformHelper);
        box.localRotation = Quaternion.identity;
        box.localPosition = center;
        box.localScale = localScale;
    }

    private class PlacementHelper
    {
        public Vector3 RotationVector { get; }

        public PlacementHelper(Vector3[] anchors)
        {
            Vector2 centerPoint = GetFlatCenterPoint(anchors);
            anchors = anchors.OrderBy(item => Vector2.SignedAngle(Vector2.up, new Vector2(item.x, item.z) - centerPoint)).ToArray();
            Vector3 ab = anchors[0] - anchors[1];
            Vector3 bc = anchors[1] - anchors[2];
            Vector3 cd = anchors[2] - anchors[3];
            Vector3 da = anchors[3] - anchors[0];

            RotationVector = GetRotationVector(ab, bc, cd, da);
        }

        private Vector3 GetRotationVector(Vector3 ab, Vector3 bc, Vector3 cd, Vector3 da)
        {
            if(ab.sqrMagnitude > bc.sqrMagnitude && ab.sqrMagnitude > cd.sqrMagnitude && ab.sqrMagnitude > da.sqrMagnitude)
            {
                return ab;
            }
            if(bc.sqrMagnitude > cd.sqrMagnitude && bc.sqrMagnitude > da.sqrMagnitude)
            {
                return bc;
            }
            if(cd.sqrMagnitude > da.sqrMagnitude)
            {
                return cd;
            }
            return da;
        }

        private Vector2 GetFlatCenterPoint(Vector3[] anchors)
        {
            Vector3 sum = anchors[0] + anchors[1] + anchors[2] + anchors[3];
            sum /= 4;
            return new Vector2(sum.x, sum.z);
        }
    }

}
