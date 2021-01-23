using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToPort
{
    class Solver
    {
        private readonly HashSet<SourceCell> cells;

        public IEnumerable<ChangeToApply> GetChangesToApply()
        {
            throw new NotImplementedException();
        }
    }

    public class ChangeToApply
    {
        public SourceCell Cell { get; }
        public ICellChoice Choice { get; }
    }

    public interface ICellChoice
    {

    }

    // A grid state in the process of being solved
    public class SolveState
    {

    }

    public class SolverCellState
    {

    }

    // Contains constant information about a cell, like neighbors
    public class SourceCell
    {
        public IEnumerable<SolverCellNeighbor> Neighbors { get; }
    }

    public class SolverCellNeighbor
    {

    }
}
