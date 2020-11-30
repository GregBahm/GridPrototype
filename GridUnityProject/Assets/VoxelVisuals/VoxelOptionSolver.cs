using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelOptionSolver
{
    public VoxelOptionSolver(VoxelVisualComponent startingPoint, OptionsByDesignation optionsSource)
    {
        SolverCell sourceCell = new SolverCell(startingPoint, optionsSource);
        if(!sourceCell.SolutionFound)
        {
            throw new Exception("Could not solve options");
        }
        ApplyOptions(sourceCell);
    }

    private void ApplyOptions(SolverCell sourceCell)
    {
        throw new NotImplementedException();
    }

    public class SolverState
    {
        private readonly Dictionary<VoxelVisualComponent, List<VoxelVisualOption>> optionsTable;

        public SolverState(MainGrid grid, OptionsByDesignation optionsSource)
        {
            optionsTable = CreateBaseOptionsTable(grid, optionsSource);
        }

        private Dictionary<VoxelVisualComponent, List<VoxelVisualOption>> CreateBaseOptionsTable(MainGrid grid, OptionsByDesignation optionsSource)
        {
            throw new NotImplementedException();
        }

        public bool GetIsPotentiallyValid(VoxelVisualComponent component, VoxelVisualOption option)
        {
            throw new NotImplementedException();
        }
        public SolverState GetWithModification(VoxelVisualComponent component, VoxelVisualOption option)
        {
            throw new NotImplementedException();
        }
    }

    public class SolverCell
    {
        public VoxelVisualComponent Component { get; }
        public SolverState SolveState { get; }
        public OptionsByDesignation OptionsSource { get; }
        public bool SolutionFound { get; }
        public VoxelVisualOption Option { get; }
        public IEnumerable<SolverCell> Consequences { get; }

        public SolverCell(VoxelVisualComponent component, OptionsByDesignation optionsSource, SolverState solveState)
        {
            Component = component;
            SolveState = solveState;
            OptionsSource = optionsSource;
            VoxelVisualOption[] options = GetOptions();
            foreach (VoxelVisualOption option in options)
            {
                Consequences = TryWithOption(option);
                if (Consequences != null)
                    break;
            }
            SolutionFound = Consequences != null;
        }

        private VoxelVisualOption[] GetOptions()
        {
            VoxelVisualOption[] designationOptions = OptionsSource.GetOptions(Component.GetCurrentDesignation());
        }

        private IEnumerable<SolverCell> TryWithOption(VoxelVisualOption option)
        {
            SolverState newState = SolveState.GetWithModification(Component, option);
        }
    }
}
public class OptionsByDesignation
{
    private readonly Dictionary<string, VoxelVisualOption[]> optionsByDesignationKey;
    public OptionsByDesignation(VoxelBlueprint[] blueprints)
    {
        VoxelVisualOption[] allOptions = GetAllOptions(blueprints).ToArray();
        optionsByDesignationKey = GetOptionsByDesignationKey(allOptions);
    }

    public VoxelVisualOption[] GetOptions(VoxelDesignation designation)
    {
        return optionsByDesignationKey[designation.Key];
    }

    private IEnumerable<VoxelVisualOption> GetAllOptions(VoxelBlueprint[] blueprints)
    {
        foreach (VoxelBlueprint blueprint in blueprints)
        {
            IEnumerable<VoxelVisualOption> options = blueprint.GenerateVisualOptions();
            foreach (VoxelVisualOption option in options)
            {
                yield return option;
            }
        }
    }

    private Dictionary<string, VoxelVisualOption[]> GetOptionsByDesignationKey(VoxelVisualOption[] allOptions)
    {
        return allOptions.GroupBy(item => item.GetDesignationKey())
            .ToDictionary(item => item.Key, item => item.ToArray());
    }
}