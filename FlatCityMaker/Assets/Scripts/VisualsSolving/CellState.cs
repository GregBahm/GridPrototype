using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TileDefinition;

namespace VisualsSolver
{
    public class CellState
    {
        public CellConnections Connections { get; }

        public Tile CurrentChoice { get; }
        public ReadOnlyCollection<Tile> RemainingOptions { get; }

        public CellStatus Status { get; private set; }
        public IEnumerable<CellState> InvalidNeighborConnections { get; private set; }

        public CellState(IEnumerable<Tile> remainingOptions, CellConnections connections)
        {
            Connections = connections;
            RemainingOptions = remainingOptions.ToList().AsReadOnly();
            CurrentChoice = RemainingOptions[0];
        }

        public CellState FallToNextOption()
        {
            IEnumerable<Tile> newOptions = RemainingOptions.Skip(1);
            return new CellState(newOptions, Connections);
        }

        public void SetStatus(SolutionState state)
        {
            if (RemainingOptions.Count == 1)
            {
                Status = CellStatus.OnLastOption;
            }
            else
            {
                InvalidNeighborConnections = Connections.GetInvalidConnections(state).ToList();
                Status = InvalidNeighborConnections.Any() ? CellStatus.InvalidAndCanDrop : CellStatus.Valid;
            }
        }
    }
}