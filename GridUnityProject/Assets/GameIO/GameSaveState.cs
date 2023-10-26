using GameGrid;
using System;
using UnityEngine;

[Serializable]
public class GameSaveState
{
    public const string SaveFilePath = "TheSaveFile";

    public GroundSaveState Ground;
    public DesignationsSaveState Designations;

    public GameSaveState(CityBuildingMain main)
    {
        Ground = new GroundSaveState(main.MainGrid);
        Designations = new DesignationsSaveState(main.MainGrid);
    }

    public void SaveToPrefs()
    {
        string asJson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(SaveFilePath, asJson);
        PlayerPrefs.Save();
    }

    public void SaveToDisk()
    {
        string filePath = Application.dataPath + "/Save.txt";
        Debug.Log(filePath);
        string asJson = JsonUtility.ToJson(this);
        System.IO.File.WriteAllText(filePath, asJson);
    }

    public static GameSaveState Load(TextAsset defaultSave)
    {
        string data = PlayerPrefs.GetString(SaveFilePath);
        if (string.IsNullOrWhiteSpace(data))
        {
            Debug.Log("No save data found. Loading default save");
            return Load(defaultSave.text);
        }
        return Load(data);
    }

    public static GameSaveState Load(string saveState)
    {
        return JsonUtility.FromJson<GameSaveState>(saveState);
    }
}
