using GameGrid;
using System.Collections;
using System.Collections.Generic;
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
        Squarifier squarifier = new Squarifier(inputA, inputB, inputC, inputD);

        OutputA.position = new Vector3(squarifier.OutputA.x, 0, squarifier.OutputA.y);
        OutputB.position = new Vector3(squarifier.OutputB.x, 0, squarifier.OutputB.y);
        OutputC.position = new Vector3(squarifier.OutputC.x, 0, squarifier.OutputC.y);
        OutputD.position = new Vector3(squarifier.OutputD.x, 0, squarifier.OutputD.y);

        Debug.DrawLine(RawA.position, OutputA.position);
        Debug.DrawLine(RawB.position, OutputB.position);
        Debug.DrawLine(RawC.position, OutputC.position);
        Debug.DrawLine(RawD.position, OutputD.position);
    }

    private class Squarifier
    {
        public Vector2 OutputA { get; }
        public Vector2 OutputB { get; }
        public Vector2 OutputC { get; }
        public Vector2 OutputD { get; }

        public Squarifier(GroundQuad quad)
            : this(quad.Points[0].Position, quad.Points[1].Position, quad.Points[2].Position, quad.Points[3].Position)
        { }
        public Squarifier(Vector2 inputA, Vector2 inputB, Vector2 inputC, Vector2 inputD)
        {
            Vector2 ac = new Vector2(inputA.x, inputA.y) - new Vector2(inputC.x, inputC.y);
            Vector2 bd = new Vector2(inputB.x, inputB.y) - new Vector2(inputD.x, inputD.y);

            Vector2 crossBd = new Vector2(bd.y, -bd.x);

            Vector2 average = (ac + crossBd) / 2;
            Vector2 offsetA = average.normalized * 4;
            Vector2 offsetB = new Vector2(-offsetA.y, offsetA.x);
            Vector2 offsetC = new Vector2(-offsetA.x, -offsetA.y);
            Vector2 offsetD = new Vector2(offsetA.y, -offsetA.x);

            Vector2 center = (inputA + inputB + inputC + inputD) / 4;

            OutputA = offsetA + center;
            OutputB = offsetB + center;
            OutputC = offsetC + center;
            OutputD = offsetD + center;
        }
    }
}
