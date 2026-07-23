using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource _sound;
    [SerializeField] private AudioSource _music;
    [SerializeField] private AudioClip[] _sounds;
    [SerializeField] private AudioClip[] _musics;

    private Dictionary<string, AudioClip> _soundDictionary;
    private Dictionary<string, AudioClip> _musicDictionary;

    private static bool CanPlay(AudioSource audioSource)
    {
        return audioSource != null && audioSource.isActiveAndEnabled;
    }

    private void Awake()
    {
        _soundDictionary = new Dictionary<string, AudioClip>();
        for (var i = 0; i < _sounds.Length; i++)
            _soundDictionary.Add(_sounds[i].name, _sounds[i]);
        
        _musicDictionary = new Dictionary<string, AudioClip>();
        for (var i = 0; i < _musics.Length; i++)
            _musicDictionary.Add(_musics[i].name, _musics[i]);
    }

    public void PlaySound(string sound)
    {
        AudioClip audioClip = null;
        if(_soundDictionary.TryGetValue(sound, out audioClip))
        {
            if (!CanPlay(_sound))
                return;

            _sound.clip = audioClip;
            _sound.Play();
        }
        else
            Debug.LogError($"No sound {sound}");
    }
    
    public void PlaySound(string sound, string dalaySound)
    {
        AudioClip audioClip = null;
        if(_soundDictionary.TryGetValue(sound, out audioClip))
        {
            if (!CanPlay(_sound))
                return;

            _sound.clip = audioClip;
            _sound.PlayDelayed(_soundDictionary[dalaySound].length);
        }
        else
            Debug.LogError($"No sound {sound}");
    }
    
    public void PlayMusic(string music, bool isFadeOut)
    {
        AudioClip audioClip = null;
        if(_musicDictionary.TryGetValue(music, out audioClip))
        {
            if (!CanPlay(_music))
                return;

            if (isFadeOut)
            {
                var sequence = DOTween.Sequence();
                sequence.Append(_music.DOFade(0, 2.0f))
                    .AppendCallback(delegate {
                    {
                        _music.clip = audioClip;
                        _music.Play();
                    } })
                    .Append(_music.DOFade(1.0f, 2.0f));

            }
            else
            {
                _music.clip = audioClip;
                _music.volume = 1.0f;
                _music.Play();
            }
        }
        else
            Debug.LogError($"No music {music}");
    }
}
