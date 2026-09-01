using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public struct ColorSpritePair
{
    public ColorId colorId;
    public Sprite sprite;
}

[RequireComponent(typeof(AudioSource))]
public class ColorMatchGameController : MonoBehaviour
{
    [Header("Sprite Database (Assign 14 Sprites here)")]
    [SerializeField] private List<ColorSpritePair> colorSprites;
    private Dictionary<ColorId, Sprite> spriteLookup = new Dictionary<ColorId, Sprite>();

    [Header("UI References")]
    [SerializeField] private MixSlot[] mixSlots; 
    [SerializeField] private Image[] targetImages; 
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Audio Clips")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxWin;
    [SerializeField] private AudioClip sfxLose;
    [SerializeField] private AudioClip sfxToggleClick; 
    [SerializeField] private AudioClip sfxCorrect;     
    [SerializeField] private AudioClip sfxIncorrect;   

    [Header("Settings")]
    [SerializeField] private float timeLimit = 90f; 
    
    public UnityEvent OnTokenEarned;

    private ColorId[] targetColors = new ColorId[3];
    private int currentSlotIndex = 0;
    
    private float currentTime;
    private bool isPlaying = false;
    private bool hasEarnedToken = false; 

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        foreach (var pair in colorSprites)
        {
            spriteLookup[pair.colorId] = pair.sprite;
        }
        
        foreach (var slot in mixSlots)
        {
            slot.OnSlotChanged += HandleSlotChanged;
        }
    }

    private void Start() => StartGame();

    public void StartGame()
    {
        currentTime = timeLimit;
        hasEarnedToken = false;
        GenerateTargets();
        ResetAllSlots();
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying) return;

        currentTime -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(currentTime).ToString() + "s";

        if (currentTime <= 0)
        {
            currentTime = 0;
            timerText.text = "0s";
            GameOver(false);
        }
    }

    private void GenerateTargets()
    {
        for (int i = 0; i < 3; i++)
        {
            targetColors[i] = (ColorId)Random.Range(0, 14); 
            
            if (targetImages[i] != null)
            {
                targetImages[i].sprite = GetSprite(targetColors[i]);
                targetImages[i].color = Color.white;
            }
        }
    }

    private void ResetAllSlots()
    {
        currentSlotIndex = 0;
        for (int i = 0; i < mixSlots.Length; i++)
        {
            mixSlots[i].ResetSlot();
            mixSlots[i].SetActive(i == 0); 
        }
    }

    private bool IsBaseColor(ColorId id)
    {
        return (int)id >= 0 && (int)id <= 4;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void HandleSlotChanged(MixSlot activeSlot)
    {
        if (!isPlaying) return;

        activeSlot.UpdateVisual(GetSprite(activeSlot.CurrentColor));

        ColorId currentTarget = targetColors[currentSlotIndex];

        if (IsBaseColor(currentTarget))
        {
            if (activeSlot.CurrentColor == currentTarget)
            {
                AdvanceSequence(activeSlot);
            }
            else
            {
                PlaySFX(sfxIncorrect);
                ResetAllSlots();
            }
        }
        else
        {
            if (activeSlot.CurrentState == SlotState.Single)
            {
                // Play standard drop sound for the first ingredient of a 2-color mix
                PlaySFX(sfxToggleClick);
                return;
            }
            else if (activeSlot.CurrentState == SlotState.Resolved)
            {
                if (activeSlot.CurrentColor == currentTarget)
                {
                    AdvanceSequence(activeSlot);
                }
                else
                {
                    PlaySFX(sfxIncorrect);
                    ResetAllSlots();
                }
            }
        }
    }

    private void AdvanceSequence(MixSlot activeSlot)
    {
        currentSlotIndex++;

        if (currentSlotIndex >= mixSlots.Length)
        {
            GameOver(true); 
        }
        else
        {
            PlaySFX(sfxCorrect);
            activeSlot.SetActive(false);
            mixSlots[currentSlotIndex].SetActive(true);
        }
    }

    public Sprite GetSprite(ColorId id)
    {
        return spriteLookup.TryGetValue(id, out Sprite s) ? s : null;
    }

    private void GameOver(bool isWin)
    {
        isPlaying = false;
        foreach (var slot in mixSlots) slot.SetActive(false); 

        if (isWin)
        {
            if (!hasEarnedToken)
            {
                hasEarnedToken = true;
                Debug.Log("Token earned"); 
                OnTokenEarned?.Invoke();
            }

            PlaySFX(sfxWin);
            GameResultManager.Instance.ShowWin();
        }
        else
        {
            PlaySFX(sfxLose);
            GameResultManager.Instance.ShowLose();
        }
    }

    private void OnDestroy()
    {
        foreach (var slot in mixSlots)
        {
            if (slot != null) slot.OnSlotChanged -= HandleSlotChanged;
        }
    }
}