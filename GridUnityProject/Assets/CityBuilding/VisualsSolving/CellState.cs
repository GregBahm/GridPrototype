using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace VisualsSolving
{
    public class CellState
    {
        private readonly VisualsSolver solver;
        public VisualCell Component { get; }

        public ReadOnlyCollection<VisualCellOption> RemainingOptions { get; }

        private readonly HashSet<VoxelConnectionType> up;
        private readonly HashSet<VoxelConnectionType> down;
        private readonly HashSet<VoxelConnectionType> left;
        private readonly HashSet<VoxelConnectionType> right;
        private readonly HashSet<VoxelConnectionType> forward;
        private readonly HashSet<VoxelConnectionType> back;

        public CellState(VisualsSolver solver, IEnumerable<VisualCellOption> remainingOptions, VisualCell component)
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

        public bool Connects(VoxelConnectionType type, ConnectionDirection direction)
        {
            switch (direction)
            {
                case ConnectionDirection.Up:
                    return down.Contains(type);
                case ConnectionDirection.Down:
                    return up.Contains(type);
                case ConnectionDirection.Left:
                    return right.Contains(type);
                case ConnectionDirection.Right:
                    return left.Contains(type);
                case ConnectionDirection.Forward:
                    return back.Contains(type);
                case ConnectionDirection.Back:
                default:
                    return forward.Contains(type);
            }
        }

        public CellState GetReducedOptions()
        {
            if (RemainingOptions.Count == 1)
            {
                throw new InvalidOperationException("Can't reduce options because there is only one remaining option");
            }

            VisualCellOption[] newRemainingOptions = RemainingOptions.Where(item => 
                solver.IsValid(item, Component)).ToArray();

            if(newRemainingOptions.Length != RemainingOptions.Count)
            {
                return new CellState(solver, newRemainingOptions, Component);
            }
            return this;
        }

        public CellState GetCollapsed()
        {
            return new CellState(solver, new VisualCellOption[] { RemainingOptions.First() }, Component);
        }

        public IEnumerable<CellState> GetNewDirtyCells(CellState oldCell)
        {
            if (solver.HasConnection(Component.Neighbors.Up) && oldCell.up.Count != up.Count)
            {
                CellState ret = solver[Component.Neighbors.Up];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (solver.HasConnection(Component.Neighbors.Down) && oldCell.down.Count != down.Count)
            {
                CellState ret = solver[Component.Neighbors.Down];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (solver.HasConnection(Component.Neighbors.Left) && oldCell.left.Count != left.Count)
            {
                CellState ret = solver[Component.Neighbors.Left];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (solver.HasConnection(Component.Neighbors.Right) && oldCell.right.Count != right.Count)
            {
                CellState ret = solver[Component.Neighbors.Right];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (solver.HasConnection(Component.Neighbors.Forward) && oldCell.forward.Count != forward.Count)
            {
                CellState ret = solver[Component.Neighbors.Forward];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
            if (solver.HasConnection(Component.Neighbors.Back) && oldCell.back.Count != back.Count)
            {
                CellState ret = solver[Component.Neighbors.Back];
                if (ret.RemainingOptions.Count > 1)
                    yield return ret;
            }
        }

        public enum ConnectionDirection
        {
            Up,
            Down,
            Left,
            Right,
            Forward,
            Back
        }
    }
}