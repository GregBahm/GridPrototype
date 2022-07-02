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

        private readonly List<VoxelRenderData> renderData;
        private ComputeBuffer renderDataBuffer;
        private int renderBufferLength = 1024; // TODO: Lower this and then make it dynamic
        private ComputeBuffer[] argsBuffers;
        private const int PositionsBufferStride = VoxelRenderData.Stride;

        public ProceduralMeshRenderer(VoxelVisualComponent component)
        {
            renderData = new List<VoxelRenderData>();
            Component = component;
            this.materials = component.Materials.Select(item => new Material(item)).ToArray();
            renderDataBuffer = new ComputeBuffer(renderBufferLength, PositionsBufferStride);
            argsBuffers = InitializeArgsBuffers();
            cellsToRender = new HashSet<VisualCell>();
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
            for (int i = 0; i < materials.Length; i++)
            {
                UpdateArgsBuffer(i);
            }
            UpdatePositionsBuffer();
        }

        public void UpdatePositionsBuffer()
        {
            renderData.Clear();
            foreach (var cell in cellsToRender)
            {
                foreach(var componet in cell.Contents.Components.Where(item => item.Component == Component))
                {
                    VoxelRenderData data = cell.GetRenderData(componet);
                    renderData.Add(data);
                }
            }
            renderDataBuffer.SetData(renderData);
        }

        private void UpdateArgsBuffer(int subMeshIndex)
        {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = Component.Mesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)renderData.Count;
            args[2] = Component.Mesh.GetIndexStart(subMeshIndex);
            args[3] = Component.Mesh.GetBaseVertex(subMeshIndex);
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