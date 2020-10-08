using GameGrid;
using System.Collections.Generic;
using System.Linq;

namespace GameGrid
{
    public class VoxelCell
    {
        private readonly MainGrid grid;
        public GroundPoint Column { get; }

        public int Height { get; }

        public bool Filled
        {
            get { return grid.IsFilled(this); }
            set
            {
                grid.SetCellFilled(this, value);
            }
        }

        public VoxelCell(MainGrid grid, GroundPoint column, int height)
        {
            this.grid = grid;
            Column = column;
            Height = height;
        }
    }
}
