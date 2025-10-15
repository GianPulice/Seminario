using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;
    private List<AudioSource> sfxSourcesPool = new List<AudioSource>(); 

    [Header("Clips")]
    [SerializeField] private Sound[] musicClips; // Lista de música con nombre
    [SerializeField] private Sound[] SFXClips;   // Lista de SFX con nombre

    private Dictionary<string, AudioClip> musicDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> SFXDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioSource> activeSFX = new Dictionary<string, AudioSource>(); 


    void Awake()
    {
        CreateSingleton(true);
        InitializeAudiosDictionaries(musicDictionary, musicClips);
        InitializeAudiosDictionaries(SFXDictionary, SFXClips);
    }


    public void PlayMusic(string musicName)
    {
        if (!musicDictionary.ContainsKey(musicName))
        {
            Debug.LogWarning("Música no encontrada: " + musicName);
            return;
        }

        musicSource.clip = musicDictionary[musicName];
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayOneShotSFX(string sfxName)
    {
        if (!SFXDictionary.ContainsKey(sfxName))
        {
            Debug.LogWarning("SFX no encontrado: " + sfxName);
            return;
        }

        SFXSource.clip = SFXDictionary[sfxName];
        SFXSource.PlayOneShot(SFXSource.clip);
    }

    public void PlaySFX(string sfxName)
    {
        if (!SFXDictionary.ContainsKey(sfxName))
        {
            Debug.LogWarning("SFX no encontrado: " + sfxName);
            return;
        }

        AudioClip clip = SFXDictionary[sfxName];
        AudioSource source = GetAvailableAudioSource();

        source.clip = clip;
        source.loop = false;
        source.Play();

        activeSFX[sfxName] = source;

        StartCoroutine(ReleaseSourceWhenDone(sfxName, source));
    }

    public void StopSFX(string sfxName)
    {
        if (activeSFX.ContainsKey(sfxName))
        {
            AudioSource src = activeSFX[sfxName];
            if (src != null && src.isPlaying)
                src.Stop();

            activeSFX.Remove(sfxName);
        }
    }


    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource src in sfxSourcesPool)
        {
            if (!src.isPlaying) return src;
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        sfxSourcesPool.Add(newSource);
        return newSource;
    }

    private System.Collections.IEnumerator ReleaseSourceWhenDone(string sfxName, AudioSource src)
    {
        yield return new WaitUntil(() => !src.isPlaying);
        if (activeSFX.ContainsKey(sfxName))
            activeSFX.Remove(sfxName);
    }

    private void InitializeAudiosDictionaries(Dictionary<string, AudioClip> audioDic, Sound[] soundType)
    {
        foreach (var sounds in soundType)
        {
            audioDic[sounds.name] = sounds.clip;
        }
    }
}


[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}
