using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class AudioManager : Singleton<AudioManager>
{
    /// <summary>
    /// Cortar las corrutinas "ReleaseSourceWhenDone" cuando pasamos de escena para que no tire errores
    /// </summary>

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSourcePlayOneShot;
    [SerializeField] private List<AudioSource> SFXSourcesPlay; 

    [Header("Clips")]
    [SerializeField] private Sound[] musicClips; // Lista de música con nombre
    [SerializeField] private Sound[] SFXClips;   // Lista de SFX con nombre

    private Dictionary<string, AudioClip> musicDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> SFXDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioSource> activeSFX = new Dictionary<string, AudioSource>();

    private string lastMusicName;
    private float lastMusicTime;


    void Awake()
    {
        CreateSingleton(true);
        SuscribeToScenesManagerEvent();
        InitializeAudiosDictionaries(musicDictionary, musicClips);
        InitializeAudiosDictionaries(SFXDictionary, SFXClips);
    }


    // ------------------------------------------ SFX ---------------------------------------------------

    public void PlayOneShotSFX(string sfxName)
    {
        if (!SFXDictionary.ContainsKey(sfxName))
        {
            Debug.LogWarning("SFX no encontrado: " + sfxName);
            return;
        }

        SFXSourcePlayOneShot.clip = SFXDictionary[sfxName];
        SFXSourcePlayOneShot.PlayOneShot(SFXSourcePlayOneShot.clip);
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
            {
                src.Stop();
                src.clip = null;
                src.pitch = 1;
                activeSFX.Remove(sfxName);
            }
        }
    }

    public void PlayLoopSFX(string sfxName, float pitch = 1f)
    {
        if (!SFXDictionary.ContainsKey(sfxName))
        {
            Debug.LogWarning("SFX no encontrado: " + sfxName);
            return;
        }

        AudioSource loopSource = GetAvailableAudioSource();
        loopSource.clip = SFXDictionary[sfxName];
        loopSource.loop = true;
        loopSource.pitch = pitch;
        loopSource.Play();

        activeSFX[sfxName] = loopSource;
    }

    public void StopLoopSFX(string sfxName)
    {
        if (activeSFX.ContainsKey(sfxName))
        {
            AudioSource source = activeSFX[sfxName];
            source.Stop();
            source.loop = false;
            source.pitch = 0f;
            source.clip = null;

            activeSFX.Remove(sfxName);
        }
    }

    public AudioClip GetSFX(string sfxName)
    {
        if (SFXDictionary.ContainsKey(sfxName))
        {
            return SFXDictionary[sfxName];
        }

        Debug.LogWarning("SFX no encontrado: " + sfxName);
        return null;
    }

    public AudioSource GetActiveSFX(string sfxName)
    {
        if (activeSFX.ContainsKey(sfxName))
        {
            return activeSFX[sfxName];
        }

        return null;
    }


    // ------------------------------------------ MUSIC ---------------------------------------------------

    public IEnumerator PlayMusic(string musicName)
    {
        yield return new WaitUntil(() => ScenesManager.Instance != null && !ScenesManager.Instance.IsInLoadingScenePanel && !ScenesManager.Instance.IsInExitGamePanel);

        if (!musicDictionary.ContainsKey(musicName))
        {
            Debug.LogWarning("Música no encontrada: " + musicName);
            yield break;
        }

        musicSource.clip = musicDictionary[musicName];
        musicSource.Play();
    }

    public void StopMusic(string musicName)
    {
        if (!musicDictionary.ContainsKey(musicName))
        {
            Debug.LogWarning("Música no encontrada: " + musicName);
            return;
        }

        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PauseCurrentMusic()
    {
        if (musicSource.isPlaying)
        {
            lastMusicName = musicSource.clip.name;
            lastMusicTime = musicSource.time;
            musicSource.Stop(); 
        }
    }

    public void ResumeLastMusic()
    {
        if (musicDictionary.ContainsKey(lastMusicName))
        {
            musicSource.clip = musicDictionary[lastMusicName];
            musicSource.time = lastMusicTime;
            musicSource.Play();
        }
    }


    private void SuscribeToScenesManagerEvent()
    {
        ScenesManager.Instance.OnSceneLoadedEvent += OnStopAllAudiosInGame;
    }

    private void OnStopAllAudiosInGame()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }

        foreach (AudioSource src in SFXSourcesPlay)
        {
            if (src.isPlaying)
            {
                src.Stop();
                src.loop = false;
                src.pitch = 1;
                src.clip = null;
            }
        }

        activeSFX.Clear();
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource src in SFXSourcesPlay)
        {
            if (!src.isPlaying) return src;
        }

        return null;
    }

    private IEnumerator ReleaseSourceWhenDone(string sfxName, AudioSource src)
    {
        yield return new WaitUntil(() => !src.isPlaying);

        if (activeSFX.ContainsKey(sfxName))
        {
            src.clip = null;
            activeSFX.Remove(sfxName);
        }
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
