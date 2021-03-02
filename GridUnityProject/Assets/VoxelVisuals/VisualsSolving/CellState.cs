using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VisualsSolving
{
    public class CellState
    {
        public VoxelVisualComponent Component { get; }

        public VoxelVisualOption CurrentChoice { get; }
        public ReadOnlyCollection<VoxelVisualOption> RemainingOptions { get; }

        public CellStatus Status { get; private set; }
        public IEnumerable<CellState> InvalidNeighborConnections { get; private set; }

        public CellState(IEnumerable<VoxelVisualOption> remainingOptions, VoxelVisualComponent component)
        {
            Component = component;
            RemainingOptions = remainingOptions.ToList().AsReadOnly();
            CurrentChoice = RemainingOptions[0];
        }

        public CellState FallToNextOption()
        {
            IEnumerable<VoxelVisualOption> newOptions = RemainingOptions.Skip(1);
            return new CellState(newOptions, Component);
        }

        public void SetStatus(SolutionState state)
        {
            if (RemainingOptions.Count == 1)
            {
                Status = CellStatus.OnLastOption;
            }
            else
            {
                InvalidNeighborConnections = Component.GetInvalidConnections(state).ToList();
                Status = InvalidNeighborConnections.Any() ? CellStatus.InvalidAndCanDrop : CellStatus.Valid;
            }
        }
    }
}