using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace SudokuStyle
{
    public class SudokuStyleSolver
    {
        private Dictionary<VoxelVisualComponent, CellState> cellStateLookup;
        public readonly HashSet<CellState> unsolvedCells;
        public readonly HashSet<CellState> dirtyCells = new HashSet<CellState>();
        public List<CellState> ReadyToDisplayVoxels { get; } = new List<CellState>();

        public bool SolveComplete
        {
            get
            {
                return unsolvedCells.Count == 0;
            }
        }

        public CellState this[VoxelVisualComponent component]
        {
            get { return cellStateLookup[component]; }
        }

        public SudokuStyleSolver(MainGrid grid, OptionsByDesignation optionsSource)
        {
            cellStateLookup = GetInitialDictionary(grid, optionsSource);
            foreach (CellState item in cellStateLookup.Values)
            {
                if(item.RemainingOptions.Count == 1)
                {
                    ReadyToDisplayVoxels.Add(item);
                }
                else
                {
                    dirtyCells.Add(item);
                }
            }
            unsolvedCells = new HashSet<CellState>(dirtyCells);
        }

        public void StepForward()
        {
            if(dirtyCells.Any())
            {
                SudokuACell();
            }
            else
            {
                CollapseACell();
            }
        }

        // Takes the first dirty cell
        // Removes each option from the cell that is no longe possible
        // If this changes the possibility of the cell, its neighbors are made dirty
        private void SudokuACell()
        {
            CellState dirtyCell = dirtyCells.First();
            dirtyCells.Remove(dirtyCell);
            unsolvedCells.Remove(dirtyCell);
            CellState cleanCell = dirtyCell.GetReducedOptions();
            cellStateLookup[cleanCell.Component] = cleanCell;
            if (cleanCell.RemainingOptions.Count > 1)
            {
                unsolvedCells.Add(cleanCell);
            }
            else
            {
                ReadyToDisplayVoxels.Add(cleanCell);
            }
            foreach (CellState cell in cleanCell.GetNewDirtyCells(dirtyCell))
            {
                dirtyCells.Add(cell);
            }
        }

        // If we've sudokued way every invalid option, we collapse an uncollapsed cell to its first choice
        // We then dirty its neighbors if necessary
        private void CollapseACell()
        {
            CellState toCollapse = unsolvedCells.First();
            unsolvedCells.Remove(toCollapse);
            dirtyCells.Remove(toCollapse);
            CellState collapsed = toCollapse.GetCollapsed();
            cellStateLookup[collapsed.Component] = collapsed;
            ReadyToDisplayVoxels.Add(collapsed);
            foreach (CellState cell in collapsed.GetNewDirtyCells(toCollapse))
            {
                dirtyCells.Add(cell);
            }
        }

        private Dictionary<VoxelVisualComponent, CellState> GetInitialDictionary(MainGrid grid, OptionsByDesignation optionsSource)
        {
            Dictionary<VoxelVisualComponent, CellState> ret = new Dictionary<VoxelVisualComponent, CellState>();
            foreach (VoxelVisualComponent component in grid.Voxels.SelectMany(item => item.Visuals.Components))
            {
                VoxelVisualOption[] options = optionsSource.GetOptions(component.GetCurrentDesignation());
                CellState state = new CellState(this, options, component);
                ret.Add(component, state);
            }
            return ret;
        }


        public bool IsValid(VoxelVisualOption option, VoxelVisualComponent component)
        {
            return (component.Neighbors.Up == null ||
                cellStateLookup[component.Neighbors.Up].ConnectsDown(option.Connections.Up))
                && (component.Neighbors.Down == null ||
                cellStateLookup[component.Neighbors.Down].ConnectsUp(option.Connections.Down))
                && (component.Neighbors.Left == null ||
                cellStateLookup[component.Neighbors.Left].ConnectsRight(option.Connections.Left))
                && (component.Neighbors.Right == null ||
                cellStateLookup[component.Neighbors.Right].ConnectsLeft(option.Connections.Right))
                && (component.Neighbors.Forward == null ||
                cellStateLookup[component.Neighbors.Forward].ConnectsBack(option.Connections.Forward))
                && (component.Neighbors.Backward == null ||
                cellStateLookup[component.Neighbors.Backward].ConnectsForward(option.Connections.Back));
        }
    }

    public class CellState
    {
        private readonly SudokuStyleSolver solver;
        public VoxelVisualComponent Component { get; }

        public ReadOnlyCollection<VoxelVisualOption> RemainingOptions { get; }

        private readonly HashSet<VoxelConnectionType> up;
        private readonly HashSet<VoxelConnectionType> down;
        private readonly HashSet<VoxelConnectionType> left;
        private readonly HashSet<VoxelConnectionType> right;
        private readonly HashSet<VoxelConnectionType> forward;
        private readonly HashSet<VoxelConnectionType> back;

        public CellState(SudokuStyleSolver solver, IEnumerable<VoxelVisualOption> remainingOptions, VoxelVisualComponent component)
        {
            this.solver = solver;
            Component = component;
            RemainingOptions = remainingOptions.ToList().AsReadOnly();
            up = new HashSet<VoxelConnectionType>(RemainingOptions.Select(item => item.Connections.Up));
            down = new HashSet<VoxelConnectionType>(RemainingOptions.Select(item => item.Connections.Down));
            left = new HashSet<VoxelConnectionType>(RemainingOptions.Select(item => item.Connections.Left));
            right = new HashSet<VoxelConnectionType>(RemainingOptions.Select(item => item.Connections.Right));
            forward = new HashSet<VoxelConnectionType>(RemainingOptions.Select(item => item.Connections.Forward));
            back = new HashSet<VoxelConnectionType>(RemainingOptions.Select(item => item.Connections.Back));
        }

        public bool ConnectsUp(VoxelConnectionType type) { return up.Contains(type); }
        public bool ConnectsDown(VoxelConnectionType type) { return down.Contains(type); }
        public bool ConnectsLeft(VoxelConnectionType type) { return left.Contains(type); }
        public bool ConnectsRight(VoxelConnectionType type) { return right.Contains(type); }
        public bool ConnectsForward(VoxelConnectionType type) { return forward.Contains(type); }
        public bool ConnectsBack(VoxelConnectionType type) { return back.Contains(type); }

        public CellState GetReducedOptions()
        {
            if (RemainingOptions.Count == 1)
            {
                throw new InvalidOperationException("Can't reduce options because there is only one remaining option");
            }

            VoxelVisualOption[] newRemainingOptions = RemainingOptions.Where(item => 
                solver.IsValid(item, Component)).ToArray();

            if(newRemainingOptions.Length != RemainingOptions.Count)
            {
                return new CellState(solver, newRemainingOptions, Component);
            }
            return this;
        }

        public CellState GetCollapsed()
        {
            return new CellState(solver, new VoxelVisualOption[] { RemainingOptions.First() }, Component);
        }

        public IEnumerable<CellState> GetNewDirtyCells(CellState oldCell)
        {
            if (Component.Neighbors.Up != null && oldCell.up.Count != up.Count)
            {
                CellState ret = solver[Component.Neighbors.Up];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (Component.Neighbors.Down != null && oldCell.down.Count != down.Count)
            {
                CellState ret = solver[Component.Neighbors.Down];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (Component.Neighbors.Left != null && oldCell.left.Count != left.Count)
            {
                CellState ret = solver[Component.Neighbors.Left];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (Component.Neighbors.Right != null && oldCell.right.Count != right.Count)
            {
                CellState ret = solver[Component.Neighbors.Right];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (Component.Neighbors.Forward != null && oldCell.forward.Count != forward.Count)
            {
                CellState ret = solver[Component.Neighbors.Forward];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (Component.Neighbors.Backward != null && oldCell.back.Count != back.Count)
            {
                CellState ret = solver[Component.Neighbors.Backward];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
        }
    }
}