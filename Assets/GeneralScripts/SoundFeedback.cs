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

    private void Awake()
    {
        if (playOnButtonClick && TryGetComponent<Button>(out Button button))
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    public void PlaySound()
    {
        if (AudioFile != null && AudioManager.Instance != null)
        {
            float randomPitch = Random.Range(MinPitch, MaxPitch);
            AudioManager.Instance.PlaySFX(AudioFile, Volume, randomPitch);
        }
    }
}