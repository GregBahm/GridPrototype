using GameGrid;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace VisualsSolving
{
    /*
     *  Warning: This threading code relies on the implementation details of GameMain to prevent deadlocking.
     *  The structure of the threading architecture is as follows:
     *  On GameMain update, we check for a changed voxel. If we have one, we set changedVoxel for discovery by VisualsSolvingManager.MainLoop
     *  Otherwise, GameMain checks the VisualsSolvingManager for ChangedVoxels to apply (and sets the field to null when they are found).
     *  
     *  In this object's MainLoop, we check for a changed voxel. If we have one, we reset the solver and start solving for Changed Voxels.
     *  When we have ChangedVoxels we set it to the field for discovery by GameMain.Update()
     */
    public class VisualsSolvingManager
    {
        private Thread thread;

        private volatile bool continueLooping = true;
        public volatile IEnumerable<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>> ChangedVoxels;
        private volatile VoxelCell changedVoxel;

        private VisualsSolver currentSolver;
        private SolutionState lastSolution;

        public VisualsSolvingManager(MainGrid grid, OptionsByDesignation optionsSource)
        {
            currentSolver = new VisualsSolver(grid, optionsSource);
            lastSolution = currentSolver.FirstState;
            thread = new Thread(MainLoop);
            thread.Start();
        }

        public void RegisterChangedVoxel(VoxelCell changedVoxel)
        {
            this.changedVoxel = changedVoxel;
        }

        public void MainLoop()
        {
            while (continueLooping)
            {
                if (changedVoxel != null)
                {
                    ResetSolver();
                    UpdateVisualsSolver();
                }
                else if (currentSolver.Status == VisualsSolver.SolverStatus.Solving)
                {
                    UpdateVisualsSolver();
                }
            }
        }

        private void UpdateVisualsSolver()
        {
            currentSolver.AdvanceOneStep();
            if (currentSolver.Status != VisualsSolver.SolverStatus.Solving)
            {
                IEnumerable<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>> changedVoxels = GetChangedVoxels();
                lastSolution = currentSolver.LastState;
                ChangedVoxels = changedVoxels;
            }
        }

        private IEnumerable<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>> GetChangedVoxels()
        {
            Dictionary<VoxelVisualComponent, VoxelVisualOption> lastState = lastSolution.GetDictionary();
            Dictionary<VoxelVisualComponent, VoxelVisualOption> currentState = currentSolver.LastState.GetDictionary();

            List<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>> ret = new List<KeyValuePair<VoxelVisualComponent, VoxelVisualOption>>();
            foreach (KeyValuePair<VoxelVisualComponent, VoxelVisualOption> entry in currentState)
            {
                if (lastState[entry.Key] != currentState[entry.Key])
                {
                    ret.Add(entry);
                }
            }
            return ret;
        }

        private void ResetSolver()
        {
            VoxelCell changed = changedVoxel;
            currentSolver.UpdateForChangedVoxel(changed);
            changedVoxel = null;
        }

        public void Destroy()
        {
            continueLooping = false;
            thread.Abort();
        }

        private class VisualsSolver
        {
            private const int SolverLimit = 10000;
            public List<SolutionState> StateHistory { get; }
            public SolutionState FirstState { get { return StateHistory.First(); } }
            public SolutionState LastState { get { return StateHistory.Last(); } }

            public SolverStatus Status { get; private set; }

            public VisualsSolver(MainGrid grid, OptionsByDesignation optionsSource)
            {
                SolutionState initialState = new SolutionState(grid, optionsSource);
                StateHistory = new List<SolutionState>() { initialState };
                Status = GetSolverStatus();
            }

            public void UpdateForChangedVoxel(VoxelCell changedVoxel)
            {
                SolutionState state = FirstState.GetWithChangedCell(changedVoxel);
                StateHistory.Clear();
                StateHistory.Add(state);
            }

            private SolverStatus GetSolverStatus()
            {
                if (StateHistory.Any() && LastState.IsEverythingSolved)
                    return SolverStatus.Solved;
                if (StateHistory.Count >= SolverLimit)
                    return SolverStatus.AtLimit;
                return SolverStatus.Solving;
            }

            public void AdvanceOneStep()
            {
                if (StateHistory.Count < SolverLimit && !LastState.IsEverythingSolved)
                {
                    SolutionState nextState = LastState.GetNextState();
                    StateHistory.Add(nextState);
                    Status = GetSolverStatus();
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
}