using GameGrid;
using System;
using System.Collections.Generic;

[Serializable]
public class DesignationsSaveState
{
    public DesignationCellSaveState[] DesignationStates;

    public DesignationsSaveState(MainGrid grid)
    {
        List<DesignationCellSaveState> designationStates = new List<DesignationCellSaveState>();
        foreach (GroundPoint point in grid.Points)
        {
            for (int height = 0; height < grid.MaxHeight; height++)
            {
                VoxelDesignationType designation = point.DesignationCells[height].Designation;
                if(designation != VoxelDesignationType.Empty)
                {
                    DesignationCellSaveState state = new DesignationCellSaveState();
                    state.GroundPointIndex = point.Index;
                    state.Height = height;
                    state.Designation = designation;
                    designationStates.Add(state);
                }
            }
        }
        DesignationStates = designationStates.ToArray();
    }

    [Serializable]
    public class DesignationCellSaveState
    {
        public int GroundPointIndex;
        public int Height;
        public VoxelDesignationType Designation;
    }
}