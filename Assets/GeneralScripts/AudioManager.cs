using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource backgroundAudioSource;

    private const string BGM_VOLUME_KEY = "BGM_Volume_Save";
    private float currentBgmVolume = 1f;
    
    private SceneBGM.BGMTrack[] currentSceneBgmList;
    private int lastPlayedIndex = -1;
    private Coroutine bgmLoopCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (backgroundAudioSource == null)
            {
                backgroundAudioSource = GetComponent<AudioSource>();
                if (backgroundAudioSource == null)
                {
                    backgroundAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadVolume();
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneChange; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneChange; }

    private void Start()
    {
        AudioListener.volume = 1f;
        CheckAndPlaySceneBGM();
    }

    private void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        CheckAndPlaySceneBGM();
    }

    private void CheckAndPlaySceneBGM()
    {
        SceneBGM sceneBGM = FindFirstObjectByType<SceneBGM>();

        if (sceneBGM != null && sceneBGM.bgmList != null && sceneBGM.bgmList.Length > 0)
        {
            currentSceneBgmList = sceneBGM.bgmList;
            lastPlayedIndex = -1;

            StartBgmPlaylist();
        }
        else
        {
            StopMusic();
        }
    }

    private void StartBgmPlaylist()
    {
        if (bgmLoopCoroutine != null)
        {
            StopCoroutine(bgmLoopCoroutine);
        }
        bgmLoopCoroutine = StartCoroutine(PlayRandomBgmRoutine());
    }

    private IEnumerator PlayRandomBgmRoutine()
    {
        while (currentSceneBgmList != null && currentSceneBgmList.Length > 0)
        {
            int randomIndex = GetRandomClipIndex();
            lastPlayedIndex = randomIndex;

            SceneBGM.BGMTrack trackToPlay = currentSceneBgmList[randomIndex];

            if (trackToPlay.clip != null)
            {
                backgroundAudioSource.clip = trackToPlay.clip;
                backgroundAudioSource.loop = false;
                
                backgroundAudioSource.volume = trackToPlay.baseVolume * currentBgmVolume;
                backgroundAudioSource.Play();

                yield return new WaitForSecondsRealtime(trackToPlay.clip.length);
            }
            else
            {
                yield return null;
            }
        }
    }

    private int GetRandomClipIndex()
    {
        if (currentSceneBgmList.Length == 1) return 0;

        int randomIndex = Random.Range(0, currentSceneBgmList.Length);

        while (randomIndex == lastPlayedIndex)
        {
            randomIndex = Random.Range(0, currentSceneBgmList.Length);
        }

        return randomIndex;
    }

    public void StopMusic()
    {
        if (bgmLoopCoroutine != null)
        {
            StopCoroutine(bgmLoopCoroutine);
            bgmLoopCoroutine = null;
        }
        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.Stop();
        }
        currentSceneBgmList = null;
    }

    public void IncreaseVolume()
    {
        currentBgmVolume = Mathf.Clamp01(currentBgmVolume + 0.1f);
        UpdateSourceVolume();
    }

    public void DecreaseVolume()
    {
        currentBgmVolume = Mathf.Clamp01(currentBgmVolume - 0.1f);
        UpdateSourceVolume();
    }

    private void UpdateSourceVolume()
    {
        if (backgroundAudioSource != null && lastPlayedIndex >= 0 && currentSceneBgmList != null && lastPlayedIndex < currentSceneBgmList.Length)
        {
            backgroundAudioSource.volume = currentSceneBgmList[lastPlayedIndex].baseVolume * currentBgmVolume;
        }
        else if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume = currentBgmVolume;
        }

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, currentBgmVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolume()
    {
        currentBgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        if (backgroundAudioSource != null)
            backgroundAudioSource.volume = currentBgmVolume;
    }

    public string GetCurrentMusicName()
    {
        if (backgroundAudioSource == null || backgroundAudioSource.clip == null) return "No Music";
        return backgroundAudioSource.clip.name;
    }
}