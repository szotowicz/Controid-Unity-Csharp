using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CheckSettingsInMainMenu : MonoBehaviour
{
    private string gameSettingsFileName = "/gamesettings.json";

    public void OnEnable()
    {
        if (!File.Exists(Application.persistentDataPath + gameSettingsFileName))
        {
            CreateNewSettingsFile();
        }

        LoadGameSettingsAfterStarting();
    }

    private void CreateNewSettingsFile()
    {
        GameSettings gameSettings = new GameSettings
        {
            MusicIsEnable = true,
            SoundEffectIsEnable = true,
            MusicVolume = 0.6f,
            CurrentResolution = Screen.currentResolution.ToString(),
            TextureQualityIndex = 0
        };

        string jsonData = JsonUtility.ToJson(gameSettings, true);
        File.WriteAllText(Application.persistentDataPath + gameSettingsFileName, jsonData);
    }

    private void LoadGameSettingsAfterStarting()
    {
        GameSettings gameSettings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(Application.persistentDataPath + gameSettingsFileName));

        // If saved resolution is not empty set this size
        Resolution[] allResolutions = Screen.resolutions;
        foreach (Resolution resolution in allResolutions)
        {
            if (resolution.ToString().Contains(gameSettings.CurrentResolution))
            {
                Screen.SetResolution(resolution.width, resolution.height, true);
                break;
            }
        }
    }
}