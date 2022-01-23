using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    int displayPointResolution = 10;
    DisplayPoint[] displayPoints;
    public Transform A;
    public Transform B;
    public Transform C;
    public Transform D;
    public Transform ControlRod;

    public void Start()
    {
        displayPoints = CreateDisplayPoints();
    }

    public void Update()
    {
        Debug.DrawRay(ControlRod.position, ControlRod.forward, Color.blue);
        Debug.DrawRay(ControlRod.position, ControlRod.right, Color.red);
        foreach (DisplayPoint point in displayPoints)
        {
            UpdatePoint(point);
        }
    }

    private void UpdatePoint(DisplayPoint point)
    {
        Vector2 a = new Vector2(A.position.x, A.position.z);
        Vector2 b = new Vector2(B.position.x, B.position.z);
        Vector2 c = new Vector2(C.position.x, C.position.z);
        Vector2 d = new Vector2(D.position.x, D.position.z);
        Vector2 position = GetRemapped(new Vector2(point.xParam, 1 - point.yParam), a, b, c, d);

        Vector2 bc = (b - c).normalized;
        Vector2 ad = (a - d).normalized;
        Vector2 ab = (a - b).normalized;
        Vector2 dc = (d - c).normalized;

        Vector2 zNormal = Vector2.Lerp(bc, ad, point.xParam);
        Vector2 xNormal = Vector2.Lerp(ab, dc, point.yParam);

        Vector2 newA = xNormal + zNormal;
        Vector2 newB = zNormal;
        Vector2 newC = Vector2.zero;
        Vector2 newD = xNormal;

        Vector2 zToRemap = new Vector2(ControlRod.forward.x, ControlRod.forward.z);
        Vector2 xToRemap = new Vector2(ControlRod.right.x, ControlRod.right.z);

        Vector2 zRemapped = GetRemapped(zToRemap, newA, newB, newC, newD);
        Vector2 xRemapped = GetRemapped(xToRemap, newA, newB, newC, newD);

        Vector3 zToDraw = new Vector3(zRemapped.x, 0, zRemapped.y) * .05f;
        Vector3 xToDraw = new Vector3(xRemapped.x, 0, xRemapped.y) * .05f;
        Vector3 positionToDraw = new Vector3(position.x, 0, position.y);
        Debug.DrawRay(positionToDraw, zToDraw, Color.blue);
        Debug.DrawRay(positionToDraw, xToDraw, Color.red);
        Debug.DrawRay(positionToDraw, new Vector3(0, .01f, 0), Color.green);
    }
    public static Vector2 GetRemapped(Vector2 point, Vector2 x1y1, Vector2 x0y1, Vector2 x0y0, Vector2 x1y0)
    {
        Vector2 y0 = Vector2.LerpUnclamped(x0y0, x1y0, point.x);
        Vector2 y1 = Vector2.LerpUnclamped(x0y1, x1y1, point.x);
        return Vector2.LerpUnclamped(y0, y1, point.y);
    }

    private DisplayPoint[] CreateDisplayPoints()
    {
        DisplayPoint[] ret = new DisplayPoint[displayPointResolution * displayPointResolution];
        int i = 0;
        for (int x = 0; x < displayPointResolution; x++)
        {
            for (int y = 0; y < displayPointResolution; y++)
            {
                float xParam = (float)x / displayPointResolution;
                float yParam = (float)y / displayPointResolution;
                ret[i] = new DisplayPoint(xParam, yParam);
                i++;
            }
        }
        return ret;
    }

    public class DisplayPoint
    {
        public float xParam { get; }
        public float yParam { get; }
        private Vector2 AsPoint { get; }

        public DisplayPoint(float xParam, float yParam)
        {
            this.xParam = xParam;
            this.yParam = yParam;
            AsPoint = new Vector2(xParam, this.yParam);
        }

        internal Vector2 GetPosition(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return GetRemapped(AsPoint, a, b, c, d);
        }
    }
}
