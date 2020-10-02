using GameGrid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameMain))]
public class GridModification : MonoBehaviour
{
    private MainGrid mainGrid;

    private void Start()
    {
        mainGrid = GetComponent<GameMain>().MainGrid;
    }

    public void DoGridModification()
    {

    }
}