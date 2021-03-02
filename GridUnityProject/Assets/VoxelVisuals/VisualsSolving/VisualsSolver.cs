using GameGrid;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using System.Resources;
using System.Linq;
using System.Diagnostics;

namespace VisualsSolving
{

    public class VisualsSolver
    {
        private const int SolverLimit = 10000;
        public List<SolutionState> StateHistory { get; }
        public SolutionState FirstState { get { return StateHistory.First(); } }
        public SolutionState LastState { get { return StateHistory.Last(); } }

        public SolverStatus Status
        {
            get
            {
                if (StateHistory.Any() && LastState.IsEverythingSolved)
                    return SolverStatus.Solved;
                if (StateHistory.Count >= SolverLimit)
                    return SolverStatus.AtLimit;
                return SolverStatus.Solving;
            }
        }

        public VisualsSolver(MainGrid grid, OptionsByDesignation optionsSource)
        {
            SolutionState initialState = new SolutionState(grid, optionsSource);
            StateHistory = new List<SolutionState>() { initialState };
        }

        public void UpdateForChangedVoxel(VoxelCell changedVoxel)
        {
            UnityEngine.Debug.Log("Updating for Changed Voxel");
            SolutionState state = FirstState.GetWithChangedCell(changedVoxel);
            StateHistory.Clear();
            StateHistory.Add(state);
        }

        public void AdvanceOneStep()
        {
            if (StateHistory.Count < SolverLimit)
            {
                SolutionState nextState = LastState.GetNextState();
                StateHistory.Add(nextState);
            }
        }

        public enum SolverStatus
        {
            Solving,
            Solved,
            AtLimit
        }
    }
}