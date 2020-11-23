using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualPieceProfile : MonoBehaviour
{
    public VisualSetConnectionType CoreSet { get; }
    public VisualSetConnectionType ConectionUpward { get; }
    public VisualSetConnectionType ConnectionDownward { get; }

    public VisualSetConnectionType ConnectionX { get; }
    public VisualSetConnectionType ConnectionZ { get; }
}
