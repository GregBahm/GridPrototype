using System.Collections.Generic;
using UnityEngine;

public class TileInteractionBehavior : MonoBehaviour
{
    public int X { get; set; }
    public int Y { get; set; }

    public IEnumerable<GridCell> ConnectedCells { get; set; }
}

