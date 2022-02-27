using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheatSheetScript : MonoBehaviour
{
    public int instanceCount = 1000;
    public VoxelBlueprint[] Blueprints;
    public Material instanceMaterial;

    private TestSet[] sets;

    void Start()
    {
        sets = GetSets().ToArray();
    }

    private IEnumerable<TestSet> GetSets()
    {
        for (int i = 0; i < 10; i++)
        {
            Mesh mesh = Blueprints[i].ArtContent;
            if(mesh != null)
                yield return new TestSet(mesh, instanceMaterial, instanceCount);
        }
        foreach (VoxelBlueprint blueprint in Blueprints.Where(item => item.ArtContent != null))
        {
            Mesh mesh = blueprint.ArtContent;
            yield return new TestSet(mesh, instanceMaterial, instanceCount);
        }
    }

    void Update()
    {
        foreach (TestSet set in sets)
        {
            set.Draw();
        }
    }

    private void OnDestroy()
    {
        foreach (TestSet item in sets)
        {
            item.Dispose();
        }
    }

    private class TestSet
    {
        public Mesh mesh;
        public Material instanceMaterial;
        public ComputeBuffer positionsBuffer;
        public ComputeBuffer argsBuffer;

        public TestSet(Mesh mesh, Material materialBase, int instanceCount)
        {
            this.mesh = mesh;
            instanceMaterial = new Material(materialBase);
            positionsBuffer = CreatePositionsBuffer(instanceCount);
            argsBuffer = CreateArgsBuffer(instanceCount);
        }

        private ComputeBuffer CreateArgsBuffer(int instanceCount)
        {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            ComputeBuffer ret = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            args[0] = (uint)mesh.GetIndexCount(0);
            args[1] = (uint)instanceCount;
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);
            ret.SetData(args);
            return ret;
        }

        private ComputeBuffer CreatePositionsBuffer(int instanceCount)
        {
            ComputeBuffer ret = new ComputeBuffer(instanceCount, 16);
            Vector4[] positions = new Vector4[instanceCount];
            for (int i = 0; i < instanceCount; i++)
            {
                float x = UnityEngine.Random.value * 1000;
                float y = UnityEngine.Random.value * 1000;
                float z = UnityEngine.Random.value * 1000;
                positions[i] = new Vector4(x, y, z, 1);
            }
            ret.SetData(positions);
            return ret;
        }

        public void Draw()
        {
            instanceMaterial.SetBuffer("positionBuffer", positionsBuffer);
            Graphics.DrawMeshInstancedIndirect(mesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
        }

        public void Dispose()
        {
            positionsBuffer.Release();
            positionsBuffer = null;
            argsBuffer.Release();
            argsBuffer = null;
        }
    }
}