using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelVisuals
{
    class ProceduralMeshRenderer
    {
        private static Bounds Bounds { get; } = new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f));

        public VoxelVisualComponent Component { get; }
        private readonly Material[] materials;

        public bool IsDirty { get; private set; }

        private readonly HashSet<VisualCell> cellsToRender;
        public int CellsToRender { get { return cellsToRender.Count; } }

        private List<VoxelRenderData> renderData;
        private ComputeBuffer renderDataBuffer;
        private int renderBufferLength = 0; // TODO: Lower this and then make it dynamic
        private MeshDataForArgsBuffer[] meshData;
        private ComputeBuffer[] argsBuffers;
        private const int PositionsBufferStride = VoxelRenderData.Stride;

        public ProceduralMeshRenderer(VoxelVisualComponent component)
        {
            renderData = new List<VoxelRenderData>();
            Component = component;
            materials = component.Materials.Select(item => new Material(item)).ToArray();
            renderDataBuffer = new ComputeBuffer(renderBufferLength, PositionsBufferStride);
            meshData = GetMeshData();
            argsBuffers = InitializeArgsBuffers();
            cellsToRender = new HashSet<VisualCell>();
        }

        private MeshDataForArgsBuffer[] GetMeshData()
        {
            MeshDataForArgsBuffer[] ret = new MeshDataForArgsBuffer[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                ret[i] = new MeshDataForArgsBuffer(Component, i);
            }
            return ret;
        }

        private ComputeBuffer[] InitializeArgsBuffers()
        {
            ComputeBuffer[] ret = new ComputeBuffer[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                ret[i] = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            }
            return ret;
        }

        public void Dispose()
        {
            renderDataBuffer.Dispose();
            foreach (ComputeBuffer buffer in argsBuffers)
            {
                buffer.Dispose();
            }
        }

        public void Add(VisualCell cell)
        {
            cellsToRender.Add(cell);
            IsDirty = true;
        }

        public void Remove(VisualCell cell)
        {
            cellsToRender.Remove(cell);
            IsDirty = true;
        }

        public void UpdateBuffers()
        {
            UpdatePositionsBuffer();
            for (int i = 0; i < materials.Length; i++)
            {
                UpdateArgsBuffer(i, renderData.Count);
            }
            IsDirty = false;
        }

        public void UpdatePositionsBuffer()
        {
            renderData = GetPositionsBufferData();
            if(renderData.Count > 1024)
            {
                Debug.Log("Hey talk to me");
            }
            renderDataBuffer.SetData(renderData);
        }

        private List<VoxelRenderData> GetPositionsBufferData()
        {
            List<VoxelRenderData> ret = new List<VoxelRenderData>();
            foreach (VisualCell cell in cellsToRender)
            {
                foreach(ComponentInSet componet in cell.Contents.Components.Where(item => item.Component == Component))
                {
                    VoxelRenderData data = cell.GetRenderData(componet);
                    ret.Add(data);
                }
            }
            return ret;
        }

        private struct MeshDataForArgsBuffer
        {
            public uint IndexCountPerInstance;
            public uint StartIndexLocation;
            public uint BaseVertexLocation;

            public MeshDataForArgsBuffer(VoxelVisualComponent component, int subMeshIndex)
            {
                IndexCountPerInstance = component.Mesh.GetIndexCount(subMeshIndex);
                StartIndexLocation = component.Mesh.GetIndexStart(subMeshIndex);
                BaseVertexLocation = component.Mesh.GetBaseVertex(subMeshIndex);
            }
        }

        private void UpdateArgsBuffer(int subMeshIndex, int instanceCount)
        {
            MeshDataForArgsBuffer meshArgs = meshData[subMeshIndex];
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = meshArgs.IndexCountPerInstance;
            args[1] = (uint)instanceCount;
            args[2] = meshArgs.StartIndexLocation;
            args[3] = meshArgs.BaseVertexLocation;
            argsBuffers[subMeshIndex].SetData(args);
        }

        public void Render()
        {
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                mat.SetBuffer("_RenderDataBuffer", renderDataBuffer);
                Graphics.DrawMeshInstancedIndirect(Component.Mesh, i, mat, Bounds, argsBuffers[i]);
            }
        }
    }
}