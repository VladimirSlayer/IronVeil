using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Громкость")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

    [Header("SFX Настройки")]
    public float sfxSpatialBlend = 1f;
    public float sfxMinDistance = 1f;
    public float sfxMaxDistance = 15f;
    public List<SFXEntry> sfxLibrary = new();

    [Header("Музыка")]
    public AudioSource musicSource; 

    private Dictionary<string, AudioClip> audioDict = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildAudioDictionary();

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
        }
        PlayMusic("wise tree music");
    }

    private void BuildAudioDictionary()
    {
        foreach (var entry in sfxLibrary)
        {
            if (!string.IsNullOrEmpty(entry.key) && entry.clip != null)
            {
                if (!audioDict.ContainsKey(entry.key))
                    audioDict.Add(entry.key, entry.clip);
                else
                    Debug.LogWarning($"Повторяющийся ключ: {entry.key}");
            }
        }
    }

    public void PlaySFXAt(string key, Vector3 position)
    {
        var entry = sfxLibrary.Find(e => e.key == key);
        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning($"SFX '{key}' не найден");
            return;
        }

        GameObject temp = new GameObject($"SFX_{key}");
        temp.transform.position = position;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = entry.clip;
        source.volume = sfxVolume * entry.clipVolume; 
        source.spatialBlend = sfxSpatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = sfxMinDistance;
        source.maxDistance = sfxMaxDistance;

        source.Play();
        Destroy(temp, entry.clip.length + 0.1f);
    }

    public void PlayMusic(string key, bool loop = true)
    {
        var entry = sfxLibrary.Find(e => e.key == key);
        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning($"SFX '{key}' не найден");
            return;
        }

        musicSource.clip = entry.clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume * entry.clipVolume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
