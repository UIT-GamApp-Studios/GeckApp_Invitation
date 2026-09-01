using UnityEngine;
using UnityEngine.UI;

public class SoundFeedback : MonoBehaviour
{
    [Header("Audio Setup")]
    public AudioClip AudioFile;
    [Range(0f, 1f)] public float Volume = 1f;
    
    [Header("Pitch Randomness")]
    [Range(-3f, 3f)] public float MinPitch = 0.95f;
    [Range(-3f, 3f)] public float MaxPitch = 1.05f;

    [Header("UI Integration")]
    [SerializeField] private bool playOnButtonClick = true;
    [SerializeField] private bool playOnEnable = false;

    private void Awake()
    {
        if (playOnButtonClick && TryGetComponent<Button>(out Button button))
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            PlaySound();
        }
    }

    public void PlaySound()
    {
        if (AudioFile != null && SFXManager.Instance != null)
        {
            float randomPitch = Random.Range(MinPitch, MaxPitch);
            SFXManager.Instance.PlaySFX(AudioFile, Volume, randomPitch);
        }
    }
}