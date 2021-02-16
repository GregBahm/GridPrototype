using System.Collections;
using System.Collections.Generic;

namespace VisualsSolver
{
    public abstract class CellConnections : IEnumerable<CellConnection>
    {
        public CellConnections()
        {
        }

        public IEnumerable<CellState> GetInvalidConnections(SolutionState state)
        {
            CellState myState = state.GetCellState(this);
            foreach (CellConnection neighbor in this)
            {
                CellState theirState = state.GetCellState(neighbor.Cell);
                if (!neighbor.IsValid(myState.CurrentChoice, theirState.CurrentChoice))
                {
                    yield return theirState;
                }
            }
        }

        public abstract IEnumerator<CellConnection> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}