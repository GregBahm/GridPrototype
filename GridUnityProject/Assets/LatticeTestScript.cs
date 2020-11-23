using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LatticeTestScript : MonoBehaviour
{
    public Transform[] TargetPoints;
    private Vector3[] basePositions;
    public Transform AnchorA;
    public Transform AnchorB;
    public Transform AnchorC;
    public Transform AnchorD;

    private void Start()
    {
        basePositions = TargetPoints.Select(item => item.position).ToArray();
    }

    private void Update()
    {
        for (int i = 0; i < TargetPoints.Length; i++)
        {
            TargetPoints[i].position = TransformPoint(basePositions[i]);
        }
    }

    private Vector3 TransformPoint(Vector3 basePos)
    {
        Vector3 anchorStart = Vector3.Lerp(AnchorA.position, AnchorB.position, basePos.z);
        Vector3 anchorEnd = Vector3.Lerp(AnchorD.position, AnchorC.position, basePos.z);
        return Vector3.Lerp(anchorStart, anchorEnd, basePos.x);
    }
}
