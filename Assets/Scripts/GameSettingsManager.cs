using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class GameSettingsManager : MonoBehaviour
{
    // Sound settings
    public Toggle MusicToggle;
    public Toggle SoundEffectToggle;
    public Slider MusicVolumeSlider;

    // Display settings
    public Dropdown ResolutionDropdown;
    public Dropdown TextureQualityDropdown;

    public Button ApplyButton;

    private Resolution[] allResolutions;
    private List<Resolution> availableResolutions = new List<Resolution>();
    private GameSettings gameSettings;

    private string gameSettingsFileName = "/gamesettings.json";

    private void OnEnable()
    {
        gameSettings = new GameSettings();

        // Set delegates. Invokes a method when the value changes
        MusicToggle.onValueChanged.AddListener(delegate { ChangeMusicToggle(); });
        SoundEffectToggle.onValueChanged.AddListener(delegate { ChangeSoundEffectToggle(); });
        MusicVolumeSlider.onValueChanged.AddListener(delegate { ChangeMusicVolume(); });
        ResolutionDropdown.onValueChanged.AddListener(delegate { ChangeResolution(); });
        TextureQualityDropdown.onValueChanged.AddListener(delegate { ChangeTextureQuality(); });
        ApplyButton.onClick.AddListener(delegate { OnApplyButtonClick(); });

        allResolutions = Screen.resolutions;
        foreach (Resolution resolution in allResolutions)
        {
            // Only resolutions more than 800 x ..
            string res = resolution.ToString();
            if (Int32.Parse(res.Substring(0, res.IndexOf(' '))) >= 800)
            {
                ResolutionDropdown.options.Add(new Dropdown.OptionData(res));
                availableResolutions.Add(resolution);
            }
        }

        LoadSettings();
    }

    public void ChangeMusicToggle()
    {
        gameSettings.MusicIsEnable = MusicToggle.isOn;
    }

    public void ChangeSoundEffectToggle()
    {
        gameSettings.SoundEffectIsEnable = SoundEffectToggle.isOn;
    }

    public void ChangeMusicVolume()
    {
        gameSettings.MusicVolume = MusicVolumeSlider.value;
        // musicSource? = MusicVolumeSlider.value;
    }

    public void ChangeResolution()
    {
        Debug.Log(ResolutionDropdown.options[ResolutionDropdown.value].text);
        gameSettings.CurrentResolution = ResolutionDropdown.options[ResolutionDropdown.value].text;
        Screen.SetResolution(availableResolutions[ResolutionDropdown.value].width, availableResolutions[ResolutionDropdown.value].height, true);
    }

    public void ChangeTextureQuality()
    {
        gameSettings.TextureQualityIndex = TextureQualityDropdown.value;
        QualitySettings.masterTextureLimit = TextureQualityDropdown.value;
    }

    public void SaveSettings()
    {
        string jsonData = JsonUtility.ToJson(gameSettings, true);
        File.WriteAllText(Application.persistentDataPath + gameSettingsFileName, jsonData);
    }

    public void LoadSettings()
    {
        if (File.Exists(Application.persistentDataPath + gameSettingsFileName))
        {
            gameSettings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(Application.persistentDataPath + gameSettingsFileName));

            MusicToggle.isOn = gameSettings.MusicIsEnable;
            SoundEffectToggle.isOn = gameSettings.SoundEffectIsEnable;
            MusicVolumeSlider.value = gameSettings.MusicVolume;

            for (int i = 0; i < availableResolutions.Count; i++)
            {
                if (availableResolutions[i].ToString().Contains(gameSettings.CurrentResolution))
                {
                    ResolutionDropdown.value = i;
                    break;
                }
            }
            TextureQualityDropdown.value = gameSettings.TextureQualityIndex;

            ResolutionDropdown.RefreshShownValue();
        }
    }

    public void OnApplyButtonClick()
    {
        SaveSettings();
        SceneManager.LoadScene("MainMenu");
    }
}