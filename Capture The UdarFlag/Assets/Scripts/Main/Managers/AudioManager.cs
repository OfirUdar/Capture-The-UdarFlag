using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Audio
{
    public string name;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1f;
    [Range(-3, 3)]
    public float pitch = 1f;
    public bool isLoop = false;
}

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource _sfxAudioSource;
    [SerializeField] private AudioSource _musicAudioSource;
    [Space]
    [SerializeField] private Audio[] _audios;

    private float _sfxVolumeGame;
    public float SFXVolumeGame
    {
        get
        {
            return _sfxVolumeGame;
        }

        set
        {
            _sfxVolumeGame = value;
            _sfxAudioSource.volume = _sfxVolumeGame;
        }
    }

    private float _startMusicVolume;
    private float _musicVolumeGame;
    public float MusicVolumeGame
    {
        get
        {
            return _musicVolumeGame;
        }

        set
        {
            _musicVolumeGame = value;
            _musicAudioSource.volume = _startMusicVolume * _musicVolumeGame;
        }
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _sfxVolumeGame = PlayerPrefs.GetFloat("SoundVolume", 1f);
            _startMusicVolume = _musicAudioSource.volume;
            MusicVolumeGame = PlayerPrefs.GetFloat("MusicVolume", 1f);
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    private Audio GetAudioByName(string audioName)
    {
        Audio audio = Array.Find(_audios, audioFind => audioFind.name == audioName);
        return audio;
    }

    private void SetAudio(Audio audio)
    {
        _sfxAudioSource.clip = audio.audioClip;
        _sfxAudioSource.volume = audio.volume * SFXVolumeGame;
        _sfxAudioSource.pitch = audio.pitch;
        _sfxAudioSource.loop = audio.isLoop;
    }


    public void PlayOneShot(string audioName)
    {
        Audio audio = GetAudioByName(audioName);
        if (audio == null) { return; }

        SetAudio(audio);
        _sfxAudioSource.PlayOneShot(audio.audioClip);
    }

    public void Play(string audioName)
    {
        Audio audio = GetAudioByName(audioName);
        if (audio == null) { return; }

        SetAudio(audio);
        _sfxAudioSource.Play();

    }
    public void Stop(string audioName)
    {
        Audio audio = GetAudioByName(audioName);
        if (audio == null) { return; }

        SetAudio(audio);
        _sfxAudioSource.Stop();
    }






}
