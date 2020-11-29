using GameGrid;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelOptionSolver
{
    private readonly DesignationOptionsManager optionsManager;

    public VoxelOptionSolver(VoxelCell startingCell, DesignationOptionsManager optionsManager)
    {
        this.optionsManager = optionsManager;
    }

    public SolverCell Recursor(VoxelCell cell)
    {
        VoxelVisualComponent component = cell.Visuals.Components.First(); // How does this actually work?
        IEnumerable<VoxelVisualOption> options = optionsManager.GetAvailableOptionsFor(component.GetCurrentDesignation());
        foreach (VoxelVisualOption option in options)
        {

        }
    }

    public class SolverCell
    {
        public bool OptionWorks { get; }
        public IEnumerable<SolverCell> Consequences { get; }

        public SolverCell(SolverCell option)
        {
        }
    }
}
public class DesignationOptionsManager
{
    private readonly Dictionary<string, VoxelVisualOption[]> optionsByDesignationKey;
    public DesignationOptionsManager(VoxelBlueprint[] blueprints)
    {
        VoxelVisualOption[] allOptions = GetAllOptions(blueprints).ToArray();
        optionsByDesignationKey = GetOptionsByDesignationKey(allOptions);
    }

    public VoxelVisualOption[] GetAvailableOptionsFor(VoxelDesignation designation)
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