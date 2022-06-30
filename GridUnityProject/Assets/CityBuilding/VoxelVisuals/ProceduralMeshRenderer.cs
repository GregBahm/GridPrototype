using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelVisuals
{
    class ProceduralMeshRenderer
    {
        private static Bounds Bounds { get; } = new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f));

        public Mesh Mesh { get; }
        private readonly Material[] materials;

        public bool IsDirty { get; private set; }

        private readonly HashSet<VisualCell> cellsToRender;
        public int CellsToRender { get { return cellsToRender.Count; } }

        private ComputeBuffer renderDataBuffer;
        private int renderBufferLength = 1024; // TODO: Lower this and then make it dynamic
        private ComputeBuffer[] argsBuffers;
        private const int PositionsBufferStride = VoxelRenderData.Stride;

        public ProceduralMeshRenderer(Mesh mesh, Material[] materials)
        {
            Mesh = mesh;
            this.materials = materials.Select(item => new Material(item)).ToArray();
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
            // TODO: Fix this class to account for multiple components
            // So currently, every time a mesh renders, it needs the render data which is the rotated and flipped anchors
            // And the systme watches for cells that are rendering this, so that it can add and remove them
            // That all works fine. The only thing you need is to find where the component in the cell matches this mesh
            // Then get the render data, and set that. You will also need to update the logic for renderBuffersLength

            VoxelRenderData[] rendereData = cellsToRender.Select(item => item.GetRenderData()).ToArray();
            if (rendereData.Length > renderBufferLength)
            {
                throw new NotImplementedException("Need to handle growth of render data buffers");
            }
            renderDataBuffer.SetData(rendereData);
        }

        private void UpdateArgsBuffer(int subMeshIndex)
        {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = Mesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)cellsToRender.Count;
            args[2] = Mesh.GetIndexStart(subMeshIndex);
            args[3] = Mesh.GetBaseVertex(subMeshIndex);
            argsBuffers[subMeshIndex].SetData(args);
        }

        public void Render()
        {
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                mat.SetBuffer("_RenderDataBuffer", renderDataBuffer);
                Graphics.DrawMeshInstancedIndirect(Mesh, i, mat, Bounds, argsBuffers[i]);
            }
        }
    }
}