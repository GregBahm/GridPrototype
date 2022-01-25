using GameGrid;
using System;
using UnityEngine;

[Serializable]
public class GameSaveState
{
    private const string SaveFilePath = "TheSaveFile";

    public GroundSaveState Ground;
    public DesignationsSaveState Designations;

    public GameSaveState(CityBuildingMain main)
    {
        Ground = new GroundSaveState(main.MainGrid);
        Designations = new DesignationsSaveState(main.MainGrid);
    }

    public void Save()
    {
        string asJson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(SaveFilePath, asJson);
        PlayerPrefs.Save();
    }

    public static GameSaveState Load()
    {
        string data = PlayerPrefs.GetString(SaveFilePath);
        if (string.IsNullOrWhiteSpace(data))
        {
            Debug.Log("No save data found. Loading default grid");
            return null;
        }
        return JsonUtility.FromJson<GameSaveState>(data);
    }
}
