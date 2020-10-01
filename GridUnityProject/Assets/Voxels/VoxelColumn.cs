using System.Collections.Generic;
using System.Linq;

namespace Voxels
{
    public class VoxelColumn
{
    public VoxelCell[] Cells { get; }

    public IEnumerable<VoxelColumn> DirectConnections { get; private set; }
    public IEnumerable<VoxelColumn> DiagonalConnections { get; private set; }

    public VoxelColumn(int maxHeight)
    {
        Cells = new VoxelCell[maxHeight];
        for (int i = 0; i < maxHeight; i++)
        {
            Cells[i] = new VoxelCell(this, i);
        }
    }

    public void SetConnections(IEnumerable<VoxelColumn> directConnections, IEnumerable<VoxelColumn> indirectConnections)
    {
        DirectConnections = directConnections.ToArray();
        DiagonalConnections = indirectConnections.ToArray();
    }

    public void ConnectCells()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Cells[i].SetConnections(Cells.Length);
        }
    }
    }
}
