using System.Collections.Generic;

namespace VisualsSolver
{
    public class VisualsSolution
    {
        private const int SolverLimit = 10000;
        public List<SolutionState> StateHistory { get; }
        public SolutionState LastState { get; private set; }

        public VisualsSolution(MainGrid grid)
        {
            SolutionState initialState = new SolutionStateForFlat(this, grid);
            StateHistory = new List<SolutionState>() { initialState };
            LastState = initialState;
            //while(!LastState.IsEverythingSolved && StateHistory.Count < SolverLimit)
            //{
            //    LastState = LastState.GetNextState();
            //    StateHistory.Add(LastState);
            //}
        }

        public void AdvanceManually()
        {
            if (!LastState.IsEverythingSolved)
            {
                LastState = LastState.GetNextState();
                StateHistory.Add(LastState);
            }
        }
    }
}