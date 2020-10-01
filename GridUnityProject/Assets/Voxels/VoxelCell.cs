using System.Collections.Generic;
using System.Linq;

namespace Voxels
{
    public class VoxelCell
{
    public VoxelColumn Column { get; }

    public int Height { get; }

    public VoxelCell CellBelow { get; private set; }
    public VoxelCell CellAbove { get; private set; }
    public IEnumerable<VoxelCell> DirectSideCells { get; private set; }
    public IEnumerable<VoxelCell> DiagonalSideCells { get; private set; }

    public VoxelCell(VoxelColumn column, int height)
    {
        Column = column;
        Height = height;
    }

    public bool Filled { get; set; }

    public void SetConnections(int maxHeight)
    {
        if(Height != 0)
        {
            CellBelow = Column.Cells[Height - 1];
        }
        if(Height < maxHeight - 1)
        {
            CellAbove = Column.Cells[Height + 1];
        }
        DirectSideCells = Column.DirectConnections.Select(item => item.Cells[Height]).ToArray();
        DiagonalSideCells = Column.DiagonalConnections.Select(item => item.Cells[Height]).ToArray();
    }
}
}
