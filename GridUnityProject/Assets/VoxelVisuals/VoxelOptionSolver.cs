using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelOptionSolver
{
    private readonly MainGrid grid;
    private readonly OptionsByDesignation optionsSource;

    public VoxelOptionSolver(VoxelVisualComponent startingComponent, MainGrid grid, OptionsByDesignation optionsSource)
    {
        SolverState initialState = new SolverState();
        VoxelOptionSolver solver = new VoxelOptionSolver(startingComponent, initialState);
        this.grid = grid;
        this.optionsSource = optionsSource;
    }

    private VoxelOptionSolver(VoxelVisualComponent component, SolverState state)
    {

    }
    

    private class SolverState
    {
        // Records each voxel visual component that has been decided so far.
        private readonly Dictionary<VoxelVisualComponent, VoxelVisualOption> state;

        public SolverState()
            : this(new Dictionary<VoxelVisualComponent, VoxelVisualOption>())
        { }
        private SolverState(Dictionary<VoxelVisualComponent, VoxelVisualOption> state)
        {
            this.state = state;
        }

        public bool GetIsPotentiallyValid(VoxelVisualComponent component, VoxelVisualOption option)
        {
            //TODO: figure out validity checking for components
            throw new NotImplementedException();
        }
        public SolverState GetWithModification(VoxelVisualComponent component, VoxelVisualOption option)
        {
            Dictionary<VoxelVisualComponent, VoxelVisualOption> newState = new Dictionary<VoxelVisualComponent, VoxelVisualOption>(state);
            newState.Add(component, option);
            return new SolverState(newState);
        }
    }
}
