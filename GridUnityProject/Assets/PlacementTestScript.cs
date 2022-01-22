using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementTestScript : MonoBehaviour
{
    public Transform Box;
    public Transform PointA;
    public Transform PointB;
    public Transform PointC;
    public Transform PointD;

    private void Update()
    {
        Vector2[] anchors = new Vector2[] {
            new Vector2(PointA.position.x, PointA.position.z),
            new Vector2(PointB.position.x, PointB.position.z),
            new Vector2(PointC.position.x, PointC.position.z),
            new Vector2(PointD.position.x, PointD.position.z),
        };
        SetBoxTransform(Box, anchors);
    }

    private void SetBoxTransform(Transform box, Vector2[] anchors)
    {
        PlacementHelper helper = new PlacementHelper(anchors);
        box.forward = helper.LongestLengthPointA - helper.LongestLengthPointB;
        
    }

    private class PlacementHelper
    {
        public Vector2 LongestLengthPointA { get; }
        public Vector2 LongestLengthPointB { get; }
        public Vector2 AdditionalPointA { get; }
        public Vector2 AdditionalPointB { get; }

        public PlacementHelper(Vector2[] anchors)
        {
            Vector2 centerPoint = GetCenterPoint(anchors);
            anchors = anchors.OrderBy(item => Vector2.SignedAngle(centerPoint, item)).ToArray();
            Vector2 ab = anchors[0] - anchors[1];
            Vector2 bc = anchors[1] - anchors[2];
            Vector2 cd = anchors[2] - anchors[3];
            Vector2 da = anchors[3] - anchors[0];

            LongestLength longestLength = GetLongestLength(ab, bc, cd, da);
            switch (longestLength)
            {
                case LongestLength.AB:
                    LongestLengthPointA = anchors[0];
                    LongestLengthPointB = anchors[1];
                    AdditionalPointA = anchors[2];
                    AdditionalPointB = anchors[3];
                    break;
                case LongestLength.BC:
                    LongestLengthPointA = anchors[1];
                    LongestLengthPointB = anchors[2];
                    AdditionalPointA = anchors[3];
                    AdditionalPointB = anchors[0];
                    break;
                case LongestLength.CD:
                    LongestLengthPointA = anchors[2];
                    LongestLengthPointB = anchors[3];
                    AdditionalPointA = anchors[0];
                    AdditionalPointB = anchors[1];
                    break;
                case LongestLength.DA:
                default:
                    LongestLengthPointA = anchors[3];
                    LongestLengthPointB = anchors[0];
                    AdditionalPointA = anchors[1];
                    AdditionalPointB = anchors[2];
                    break;
            }
        }

        private LongestLength GetLongestLength(Vector2 ab, Vector2 bc, Vector2 cd, Vector2 da)
        {
            if(ab.sqrMagnitude > bc.sqrMagnitude && ab.sqrMagnitude > cd.sqrMagnitude && ab.sqrMagnitude > da.sqrMagnitude)
            {
                return LongestLength.AB;
            }
            if(bc.sqrMagnitude > cd.sqrMagnitude && bc.sqrMagnitude > da.sqrMagnitude)
            {
                return LongestLength.BC;
            }
            if(cd.sqrMagnitude > da.sqrMagnitude)
            {
                return LongestLength.CD;
            }
            return LongestLength.DA;
        }

        private enum LongestLength
        {
            AB,
            BC,
            CD,
            DA
        }
        private Vector2 GetCenterPoint(Vector2[] anchors)
        {
            Vector2 sum = anchors[0] + anchors[1] + anchors[2] + anchors[3];
            return sum / 4;
        }
    }

}
