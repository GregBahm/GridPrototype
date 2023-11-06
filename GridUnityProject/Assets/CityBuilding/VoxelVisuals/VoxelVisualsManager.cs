using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelVisuals
{
    public class VoxelVisualsManager
    {
        private readonly CityBuildingMain cityMain;
        private readonly VisualOptionsByDesignation optionsSource;

        private readonly Dictionary<VisualCell, VisualCellOption> toRemove;
        private readonly Dictionary<VisualCell, VisualCellOption> toAdd;
        private Dictionary<Mesh, ProceduralMeshRenderer> renderers;

        public VoxelVisualsManager(CityBuildingMain cityMain, VisualOptionsByDesignation optionsSource)
        {
            this.cityMain = cityMain;
            this.optionsSource = optionsSource;
            toRemove = new Dictionary<VisualCell, VisualCellOption>();
            toAdd = new Dictionary<VisualCell, VisualCellOption>();
            renderers = GetRenderers(optionsSource);
            VisualCell.ContentsChanged += OnVoxelOptionChange;
        }

        public void UpdateColumn(GroundQuad column)
        {
            bool yesStrut = false;
            for (int i = cityMain.MainGrid.MaxHeight - 1; i >= 0; i--) // Top to bottom
            {
                VisualCell cell = cityMain.MainGrid.GetVisualCell(column, i);
                VoxelVisualDesignation designation = cell.GetCurrentDesignation();
                VisualCellOptions option = optionsSource.GetOptions(designation);
                if (yesStrut)
                {
                    cell.Contents = option.UpStrutOption;
                }
                else
                {
                    cell.Contents = option.DefaultOption;
                }
                yesStrut = cell.Contents.Down == VoxelConnectionType.BigStrut;
            }
        }

        public void UpdateForBaseGridModification()
        {
            foreach (ProceduralMeshRenderer item in renderers.Values.Where(item => item.CellsToRender > 0))
            {
                item.UpdatePositionsBuffer();
            }
        }

        private Dictionary<Mesh, ProceduralMeshRenderer> GetRenderers(VisualOptionsByDesignation optionsSource)
        {
            Dictionary<Mesh, ProceduralMeshRenderer> ret = new Dictionary<Mesh, ProceduralMeshRenderer>();
            foreach (VoxelVisualComponent component in optionsSource.BaseComponents)
            {
                ProceduralMeshRenderer renderer = new ProceduralMeshRenderer(component);
                ret.Add(component.Mesh, renderer);
            }
            return ret;
        }

        private void OnVoxelOptionChange(object sender, VisualCellChangedEventArg args)
        {
            RegisterRemoval(args);
            RegisterAdd(args);
        }

        public void Update()
        {
            ProcessAllToRemoves();
            ProcessAllToAdds();
            UpdateBuffers();
            Render();
        }

        private void Render()
        {
            foreach (ProceduralMeshRenderer item in renderers.Values.Where(item => item.CellsToRender > 0))
            {
                item.Render();
            }
        }

        private void UpdateBuffers()
        {
            foreach (ProceduralMeshRenderer item in renderers.Values.Where(item => item.IsDirty))
            {
                item.UpdateBuffers();
            }
        }

        private void ProcessAllToRemoves()
        {
            foreach (KeyValuePair<VisualCell, VisualCellOption> item in toRemove)
            {
                foreach (ComponentInSet setComponent in item.Value.Components)
                {
                    ProceduralMeshRenderer renderer = renderers[setComponent.Component.Mesh];
                    renderer.Remove(item.Key);
                }
            }
            toRemove.Clear();
        }

        private void ProcessAllToAdds()
        {
            foreach (KeyValuePair<VisualCell, VisualCellOption> item in toAdd)
            {
                foreach (ComponentInSet setComponent in item.Value.Components)
                {
                    ProceduralMeshRenderer renderer = renderers[setComponent.Component.Mesh];
                    renderer.Add(item.Key);
                }
            }
            toAdd.Clear();
        }

        private void RegisterRemoval(VisualCellChangedEventArg args)
        {
            if (!toRemove.ContainsKey(args.Cell)) // Discard each change after the first as they were never applied
            {
                toRemove.Add(args.Cell, args.OldOption);
            }
        }

        private void RegisterAdd(VisualCellChangedEventArg args)
        {
            if (toAdd.ContainsKey(args.Cell))
            {
                toAdd[args.Cell] = args.Cell.Contents;
            }
            else
            {
                toAdd.Add(args.Cell, args.Cell.Contents);
            }
        }

        public void Dispose()
        {
            foreach (ProceduralMeshRenderer item in renderers.Values)
            {
                item.Dispose();
            }
        }
    }

    public class VisualCellChangedEventArg : EventArgs
    {
        public VisualCell Cell { get; }
        public VisualCellOption OldOption { get; }

        public VisualCellChangedEventArg(VisualCell cell, VisualCellOption oldOption)
        {
            if (cell == null)
                throw new ArgumentNullException("VisualCellChangedEventArg cell cannot be null");
            if (oldOption == null)
                throw new ArgumentNullException("VisualCellChangedEventArg oldOption cannot be null");
            Cell = cell;
            OldOption = oldOption;
        }
    }

    public struct VoxelRenderData
    {
        /// <summary>
        /// Stried of each voxel  renderer is 40 (could def be optimized)
        /// </summary>
        public const int Stride = sizeof(float) * 2 * 4  // Anchors
            + sizeof(float) // Height
            + sizeof(float); // FlipNormal

        public Vector2 AnchorA { get; }
        public Vector2 AnchorB { get; }
        public Vector2 AnchorC { get; }
        public Vector2 AnchorD { get; }
        public float Height { get; }
        public float FlipNormal { get; }

        public VoxelRenderData(Vector2 anchorA,
            Vector2 anchorB,
            Vector2 anchorC,
            Vector2 anchorD,
            float height,
            float flipNormal)
        {
            AnchorA = anchorA;
            AnchorB = anchorB;
            AnchorC = anchorC;
            AnchorD = anchorD;
            Height = height;
            FlipNormal = flipNormal;
        }
    }
}