using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    private string gameSettingsFileName = "/gamesettings.json";

    // Use this for initialization
    void Start () {
        if (File.Exists(Application.persistentDataPath + gameSettingsFileName))
        {
            GameSettings gameSettings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(Application.persistentDataPath + gameSettingsFileName));

            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (!gameSettings.MusicIsEnable)
            {
                if (audioSource != null)
                {
                    audioSource.mute = true;
                }
            }
            else
            {
                if (audioSource != null)
                {
                    audioSource.volume = gameSettings.MusicVolume;
                }
            }
        }
    }
}