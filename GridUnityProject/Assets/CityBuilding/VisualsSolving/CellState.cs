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

        public CellState(VisualsSolver solver, IEnumerable<VisualCellOption> remainingOptions, VisualCell component)
        {
            this.solver = solver;
            Component = component;
            RemainingOptions = remainingOptions.ToList().AsReadOnly();
            up = new HashSet<VoxelConnectionType>(RemainingOptions.Select(item => item.Connections.Up));
            down = new HashSet<VoxelConnectionType>(RemainingOptions.Select(item => item.Connections.Down));
        }

        public bool Connects(VoxelConnectionType type, ConnectionDirection direction)
        {
            switch (direction)
            {
                case ConnectionDirection.Up:
                    return down.Contains(type);
                default:
                case ConnectionDirection.Down:
                    return up.Contains(type);
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
        }

        public enum ConnectionDirection
        {
            Up,
            Down
        }
    }
}