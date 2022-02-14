using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstancingTestScript : MonoBehaviour
{
    public GameObject Prefab;
    public int HowMany;
    public Mesh Mesh;
    public Material ProceduralMat;
    private List<InstanceSet> instanceSets;

    void Start()
    {
        //DoRendererStyle();
        DoBufferStyle();
    }

    private void DoBufferStyle()
    {
        Matrix4x4[] matrixes = GetMatrixes().ToArray();
        List<List<Matrix4x4>> sets = GetMatrixSets(matrixes);
        instanceSets = sets.Select(item => new InstanceSet(Mesh, ProceduralMat, item)).ToList();
    }

    private List<List<Matrix4x4>> GetMatrixSets(Matrix4x4[] matrixes)
    {
        List<List<Matrix4x4>> ret = new List<List<Matrix4x4>>();

        List<Matrix4x4> next = new List<Matrix4x4>();
        for (int i = 0; i < matrixes.Length; i++)
        {
            next.Add(matrixes[i]);
            if(next.Count == 1023)
            {
                ret.Add(next);
                next = new List<Matrix4x4>();
            }
        }
        ret.Add(next);
        return ret;
    }

    private IEnumerable<Matrix4x4> GetMatrixes()
    {
        for (int x = 0; x < HowMany; x++)
        {
            for (int y = 0; y < HowMany; y++)
            {
                for (int z = 0; z < HowMany; z++)
                {
                    Matrix4x4 ret = Matrix4x4.identity;
                    ret.SetTRS(new Vector3(x * 1.1f, y * 1.1f, z * 1.1f), Quaternion.identity, Vector3.one);
                    yield return ret;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if(instanceSets != null)
        {
            foreach (InstanceSet set in instanceSets)
                set.OnDestroy();
        }
    }

    private void DoRendererStyle()
    {
        MaterialPropertyBlock matBlock = new MaterialPropertyBlock();
        MakeBoxes(matBlock);
    }

    private void MakeBoxes(MaterialPropertyBlock matBlock)
    {
        for (int x = 0; x < HowMany; x++)
        {
            for (int y = 0; y < HowMany; y++)
            {
                for (int z = 0; z < HowMany; z++)
                {
                    MakeOne(matBlock, x, y, z);
                }
            }
        }
    }

    void Update()
    {
        foreach (InstanceSet set in instanceSets)
        {
            set.Call();
        }
    }

    private class InstanceSet
    {
        private Mesh mesh;
        private Material mat;
        private List<Matrix4x4> transforms;
        private ComputeBuffer colorBuffer;

        public InstanceSet(Mesh mesh, Material mat, List<Matrix4x4> transforms)
        {
            this.mesh = mesh;
            this.mat = new Material(mat);
            this.transforms = transforms;
            this.colorBuffer = CreateColorBuffer();
            mat.SetBuffer("colors", this.colorBuffer);
        }

        private ComputeBuffer CreateColorBuffer()
        {
            ComputeBuffer buffer = new ComputeBuffer(transforms.Count, sizeof(float) * 4);
            Vector4[] data = new Vector4[transforms.Count];
            for (int i = 0; i < transforms.Count; i++)
            {
                data[i] = new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1);
            }
            buffer.SetData(data);
            return buffer;
        }

        public void Call()
        {
            Graphics.DrawMeshInstanced(mesh, 0, mat, transforms);
        }

        public void OnDestroy()
        {
            if (colorBuffer != null)
                colorBuffer.Dispose();
        }
    }

    private void MakeOne(MaterialPropertyBlock matBlock, int x, int y, int z)
    {
        matBlock.SetColor("_Color", new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
        GameObject obj = Instantiate(Prefab);
        obj.GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
        obj.transform.position = new Vector3(x * 1.1f, y * 1.1f, z * 1.1f);
    }
}
