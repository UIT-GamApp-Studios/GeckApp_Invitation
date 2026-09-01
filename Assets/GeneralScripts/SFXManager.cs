using UnityEngine;
using System.Collections.Generic;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    private const string SFX_VOLUME_KEY = "SFX_Volume_Save";
    private float currentSfxVolume = 1f;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 5;
    private List<AudioSource> audioSourcePool = new List<AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadVolume();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        GameObject obj = new GameObject("Pooled_AudioSource");
        obj.transform.SetParent(transform);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        audioSourcePool.Add(source);
        return source;
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        // If all are playing, expand pool
        return CreateNewAudioSource();
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableAudioSource();
        source.clip = clip;
        source.volume = volumeMultiplier * currentSfxVolume;
        source.pitch = pitch;
        source.Play();
    }

    public void IncreaseVolume()
    {
        currentSfxVolume = Mathf.Clamp01(currentSfxVolume + 0.1f);
        SaveVolume();
    }

    public void DecreaseVolume()
    {
        currentSfxVolume = Mathf.Clamp01(currentSfxVolume - 0.1f);
        SaveVolume();
    }
    
    public void SetVolume(float volume)
    {
        currentSfxVolume = Mathf.Clamp01(volume);
        SaveVolume();
    }

    private void SaveVolume()
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, currentSfxVolume);
        PlayerPrefs.Save();
        // Note: Already playing SFXs won't change volume midway in this simple setup.
    }

    private void LoadVolume()
    {
        currentSfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }
    
    // Fallback if anyone calls without multiplier
    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f, 1f);
    }
}
