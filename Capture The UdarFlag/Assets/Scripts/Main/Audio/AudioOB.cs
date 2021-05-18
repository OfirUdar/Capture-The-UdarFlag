using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class AudioOB : MonoBehaviour
{
    private AudioSource _audioSource;



    private void Awake()
    {
        _audioSource = this.GetComponent<AudioSource>();
        if (_audioSource.playOnAwake)
        {
            _audioSource.volume *= AudioManager.Instance.SFXVolumeGame;
        }
    }

    private void SetAudio(Audio audio)
    {
        _audioSource.clip = audio.audioClip;
        _audioSource.volume = audio.volume * AudioManager.Instance.SFXVolumeGame;
        _audioSource.pitch = audio.pitch;
        _audioSource.loop = audio.isLoop;
    }

    public void Play(Audio audio)
    {
        SetAudio(audio);
        _audioSource.Play();
    }
    public void Stop()
    {
        _audioSource.Stop();
    }
    public void PlayOneShot(Audio audio)
    {
        SetAudio(audio);
        _audioSource.PlayOneShot(_audioSource.clip);
    }
    public void PlayOneShot(AudioClip audioClip, float volume)
    {
        _audioSource.PlayOneShot(audioClip, volume * AudioManager.Instance.SFXVolumeGame);
    }
}
