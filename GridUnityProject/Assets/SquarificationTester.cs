using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SquarificationTester : MonoBehaviour
{
    public Transform RawA;
    public Transform RawB;
    public Transform RawC;
    public Transform RawD;

    public Transform OutputA;
    public Transform OutputB;
    public Transform OutputC;
    public Transform OutputD;

    private void Update()
    {
        Vector2 inputA = new Vector2(RawA.position.x, RawA.position.z);
        Vector2 inputB = new Vector2(RawB.position.x, RawB.position.z);
        Vector2 inputC = new Vector2(RawC.position.x, RawC.position.z);
        Vector2 inputD = new Vector2(RawD.position.x, RawD.position.z);
        Squarifier squarifier = new Squarifier(inputA, inputB, inputC, inputD, 4);

        OutputA.position = new Vector3(squarifier.OutputA.x, 0, squarifier.OutputA.y);
        OutputB.position = new Vector3(squarifier.OutputB.x, 0, squarifier.OutputB.y);
        OutputC.position = new Vector3(squarifier.OutputC.x, 0, squarifier.OutputC.y);
        OutputD.position = new Vector3(squarifier.OutputD.x, 0, squarifier.OutputD.y);

        Debug.DrawLine(RawA.position, OutputA.position);
        Debug.DrawLine(RawB.position, OutputB.position);
        Debug.DrawLine(RawC.position, OutputC.position);
        Debug.DrawLine(RawD.position, OutputD.position);
    }
    private IEnumerable<Vector2> GetSortedPoints(Vector2[] rawPoints)
    {
        Vector2 center = (rawPoints[0] + rawPoints[1] + rawPoints[2] + rawPoints[3]) / 4;
        return rawPoints.OrderByDescending(item => Vector2.SignedAngle(Vector2.up, item - center));
    }

    private class Squarifier
    {
        private readonly Vector2[] outputs;
        public Vector2 OutputA { get { return outputs[0]; } }
        public Vector2 OutputB { get { return outputs[1]; } }
        public Vector2 OutputC { get { return outputs[2]; } }
        public Vector2 OutputD { get { return outputs[3]; } }

        public Squarifier(GroundQuad quad, float size)
            : this(quad.Points[0].Position, quad.Points[1].Position, quad.Points[2].Position, quad.Points[3].Position, size)
        { }
        public Squarifier(Vector2 inputA, Vector2 inputB, Vector2 inputC, Vector2 inputD, float size)
        {
            Vector2 ac = new Vector2(inputA.x, inputA.y) - new Vector2(inputC.x, inputC.y);
            Vector2 bd = new Vector2(inputB.x, inputB.y) - new Vector2(inputD.x, inputD.y);

            Vector2 crossBd = new Vector2(bd.y, -bd.x);

            Vector2 average = (ac.normalized + crossBd.normalized) / 2;
            average = average.normalized;
            Vector2 offsetA = average;
            Vector2 offsetB = new Vector2(average.y, -average.x);
            Vector2 offsetC = -average;
            Vector2 offsetD = new Vector2(-average.y, average.x);

            Vector2 center = (inputA + inputB + inputC + inputD) / 4;

            PositionPair outputA = new PositionPair(offsetA * size + center, 0);
            PositionPair outputB = new PositionPair(offsetB * size + center, 1);
            PositionPair outputC = new PositionPair(offsetC * size + center, 2);
            PositionPair outputD = new PositionPair(offsetD * size + center, 3);
            List<PositionPair> sourceOutputs = new List<PositionPair> { outputA, outputB, outputC, outputD };

            outputs = new Vector2[4];

            PositionPair bestA = GetBest(inputA, sourceOutputs);
            outputs[0] = bestA.Pos;
            sourceOutputs.Remove(bestA);

            PositionPair bestB = GetBest(inputB, sourceOutputs);
            outputs[1] = bestB.Pos;
            sourceOutputs.Remove(bestB);

            PositionPair bestC = GetBest(inputC, sourceOutputs);
            outputs[2] = bestC.Pos;
            sourceOutputs.Remove(bestC);

            outputs[3] = sourceOutputs[0].Pos;
        }

        private class PositionPair
        {
            public int Index { get; }
            public Vector2 Pos { get; }
            public PositionPair(Vector2 pos, int index)
            {
                Index = index;
                Pos = pos;
            }
        }

        private PositionPair GetBest(Vector2 input, List<PositionPair> options)
        {
            float min = Mathf.Infinity;
            PositionPair ret = null;
            foreach (var item in options)
            {
                float dist = (input - item.Pos).sqrMagnitude;
                if(dist < min)
                {
                    min = dist;
                    ret = item;
                }
            }
            return ret;
        }
    }
}
