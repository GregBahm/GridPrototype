using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelVisuals;

namespace Interiors
{
    public class GridInteriors
    {
        private readonly Dictionary<DesignationCell, Interior> cellTable;
        private readonly Dictionary<Interior, HashSet<DesignationCell>> interiorsTable;
        public IEnumerable<Interior> Interiors { get; }

        public GridInteriors()
        {
            cellTable = new Dictionary<DesignationCell, Interior>();
            interiorsTable = new Dictionary<Interior, HashSet<DesignationCell>>();
        }

        public Interior GetFor(DesignationCell cell)
        {
            if(cellTable.ContainsKey(cell))
            {
                return cellTable[cell];
            }
            return null;
        }

        public IEnumerable<DesignationCell> GetFor(Interior interior)
        {
            return interiorsTable[interior];
        }
        public void SetInterior(DesignationCell cell, Interior interior)
        {
            if(interior == null)
            {
                if(cellTable.ContainsKey(cell))
                {
                    Interior oldInterior = cellTable[cell];
                    interiorsTable[oldInterior].Remove(cell);
                    cellTable.Remove(cell);
                }
            }
            else
            {
                if (!interiorsTable.ContainsKey(interior))
                {
                    interiorsTable.Add(interior, new HashSet<DesignationCell>());
                }
                if (cellTable.ContainsKey(cell))
                {
                    Interior oldInterior = cellTable[cell];
                    interiorsTable[oldInterior].Remove(cell);
                    cellTable[cell] = interior;
                }
                else
                {
                    cellTable.Add(cell, interior);
                }
                interiorsTable[interior].Add(cell);
            }
        }
    }
}