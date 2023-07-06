using GameGrid;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public Transform[] ConnectedPoints;
    public Transform SourcePoint;
    public Transform TargetPoint;

    private void Update()
    {
        //GroundPointEaser easer = new GroundPointEaser(SourcePoint, ConnectedPoints);
        Vector2[] points = ConnectedPoints.Select(item => new Vector2(item.position.x, item.position.z)).ToArray();
        Vector2 centeroid = GetCentroid(points);
        TargetPoint.position = new Vector3(centeroid.x, 0, centeroid.y);

        for (int i = 0; i < ConnectedPoints.Length; i++)
        {
            int next = (i + 1) % ConnectedPoints.Length;
            int nextNext = (i + 2) % ConnectedPoints.Length;
            Vector3 a = ConnectedPoints[i].position;
            Vector3 b = ConnectedPoints[next].position;
            Vector3 c = ConnectedPoints[nextNext].position;
            Debug.DrawLine(a, b);
            Debug.DrawLine(b, c);
            //Debug.DrawLine(c, a);
        }
    }
    private struct GroundPointEaser
    {
        public Vector2 OptimalPosition { get; }

        public GroundPointEaser(Transform groundPoint, Transform[] connections)
        {
            OptimalPosition = GetOptimalPosition(new Vector2(groundPoint.position.x, groundPoint.position.z), connections);
        }

        private static Vector2 GetOptimalPosition(Vector2 groundPoint, Transform[] connections)
        {
            Vector2[] connected = connections.Select(item => new Vector2(item.position.x, item.position.z)).ToArray();
            Vector2 offsetSum = Vector2.zero;
            float weigthSum = 0;
            for (int i = 0; i < connected.Length; i++)
            {
                Connection connection = GetConnection(groundPoint, connected[i]);
                offsetSum += connection.Offset * connection.Weight;
                weigthSum += connection.Weight;
            }
            offsetSum /= weigthSum;
            return offsetSum + groundPoint;
        }

        private static Connection GetConnection(Vector2 source, Vector2 target)
        {
            Vector2 diff = target - source;
            Vector2 idealPos = diff.normalized;
            float weight = Mathf.Abs(diff.magnitude);
            weight *= weight;
            return new Connection(idealPos, weight);
        }

        private struct Connection
        {
            public Vector2 Offset { get; }
            public float Weight { get; }

            public Connection(Vector2 targetPosition, float weight)
            {
                Offset = targetPosition;
                Weight = weight;
            }
        }
    }

    public Vector2 GetCentroid(Vector2[] points)
    {
        float centroidX = 0;
        float centroidY = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 currentPoint = points[i];
            Vector2 nextPoint = points[(i + 1) % points.Length];

            float commonFactor = currentPoint.x * nextPoint.y - nextPoint.x * currentPoint.y;
            centroidX += (currentPoint.x + nextPoint.x) * commonFactor;
            centroidY += (currentPoint.y + nextPoint.y) * commonFactor;
        }

        float area = GetArea(points) * 6;

        return new Vector2(centroidX / area, centroidY / area );
    }

    private float GetArea(Vector2[] points)
    {
        float area = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 currentPoint = points[i];
            Vector2 nextPoint = points[(i + 1) % points.Length];

            area += currentPoint.x * nextPoint.y;
            area -= currentPoint.y * nextPoint.x;
        }

        area /= 2;
        return area;
    }
}
