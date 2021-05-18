using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings_Panel : Panel
{
    [SerializeField] private Slider _soundVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;


    private void Start()
    {
        _soundVolumeSlider.value = PlayerPrefs.GetFloat("SoundVolume", 1f);
        _musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
    }

    public void SaveSoundVolume()
    {
        PlayerPrefs.SetFloat("SoundVolume", _soundVolumeSlider.value);
        AudioManager.Instance.SFXVolumeGame = _soundVolumeSlider.value;
    } 
    public void SaveMusicVolume()
    {
        PlayerPrefs.SetFloat("MusicVolume", _musicVolumeSlider.value);
        AudioManager.Instance.MusicVolumeGame = _musicVolumeSlider.value;
    }

}
