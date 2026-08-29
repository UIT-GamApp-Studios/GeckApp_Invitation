using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class ObjectInspectorGameController : MonoBehaviour
{
    [Header("SelectFrame Toggles (Interactable)")]
    [SerializeField] private MinigameToggleUI flyToggleUI;
    [SerializeField] private MinigameToggleUI swimToggleUI;
    [SerializeField] private MinigameToggleUI attackToggleUI;

    [Header("Hint Toggles (Read-Only)")]
    [SerializeField] private MinigameToggleUI hintFlyToggleUI;
    [SerializeField] private MinigameToggleUI hintSwimToggleUI;
    [SerializeField] private MinigameToggleUI hintAttackToggleUI;

    [Header("Streak Indicators")]
    [SerializeField] private MinigameToggleUI streak1UI;
    [SerializeField] private MinigameToggleUI streak2UI;
    [SerializeField] private MinigameToggleUI streak3UI;

    [Header("UI Visuals")]
    [SerializeField] private Image objectiveImage; 
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Audio Clips")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxWin;
    [SerializeField] private AudioClip sfxLose;
    [SerializeField] private AudioClip sfxToggleClick;
    [SerializeField] private AudioClip sfxCorrect;
    [SerializeField] private AudioClip sfxIncorrect;

    [Header("Settings & Database")]
    [SerializeField] private float timeLimit = 15f;
    [SerializeField] private int requiredStreak = 3;
    [SerializeField] private List<ObjectDefinition> objectDatabase;

    public UnityEvent OnTokenEarned;

    private ObjectDefinition currentTarget;
    private float currentTime;
    private int currentStreak = 0;
    private bool isPlaying = false;
    private bool hasEarnedToken = false;
    private int lastTargetIndex = -1;

    private void Start()
    {
        // Automatically grab the AudioSource if it wasn't assigned in the Inspector
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (hintFlyToggleUI != null && hintFlyToggleUI.Toggle != null) hintFlyToggleUI.Toggle.interactable = false;
        if (hintSwimToggleUI != null && hintSwimToggleUI.Toggle != null) hintSwimToggleUI.Toggle.interactable = false;
        if (hintAttackToggleUI != null && hintAttackToggleUI.Toggle != null) hintAttackToggleUI.Toggle.interactable = false;

        if (streak1UI != null && streak1UI.Toggle != null) streak1UI.Toggle.interactable = false;
        if (streak2UI != null && streak2UI.Toggle != null) streak2UI.Toggle.interactable = false;
        if (streak3UI != null && streak3UI.Toggle != null) streak3UI.Toggle.interactable = false;

        if (flyToggleUI != null && flyToggleUI.Toggle != null) 
            flyToggleUI.Toggle.onValueChanged.AddListener(isOn => CheckPlayerInput("Fly", isOn));
        if (swimToggleUI != null && swimToggleUI.Toggle != null) 
            swimToggleUI.Toggle.onValueChanged.AddListener(isOn => CheckPlayerInput("Swim", isOn));
        if (attackToggleUI != null && attackToggleUI.Toggle != null) 
            attackToggleUI.Toggle.onValueChanged.AddListener(isOn => CheckPlayerInput("Attack", isOn));

        StartGame();
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

    public void StartGame()
    {
        currentTime = timeLimit;
        currentStreak = 0;
        hasEarnedToken = false;
        UpdateStreakUI();
        PickNextTarget();
        isPlaying = true;
    }

    private void PickNextTarget()
    {
        int randomIndex = lastTargetIndex;
        while (randomIndex == lastTargetIndex)
        {
            randomIndex = Random.Range(0, objectDatabase.Count);
        }

        lastTargetIndex = randomIndex;
        currentTarget = objectDatabase[randomIndex];

        if (objectiveImage != null)
        {
            objectiveImage.sprite = currentTarget.objectiveSprite;
        }

        UpdateHints();

        bool randFly;
        bool randSwim;
        bool randAttack;

        do
        {
            randFly = Random.value > 0.5f;
            randSwim = Random.value > 0.5f;
            randAttack = Random.value > 0.5f;
        } 
        while (randFly == currentTarget.canFly && 
               randSwim == currentTarget.canSwim && 
               randAttack == currentTarget.canAttack);

        flyToggleUI.SetState(randFly);
        swimToggleUI.SetState(randSwim);
        attackToggleUI.SetState(randAttack);
    }

    private void UpdateHints()
    {
        hintFlyToggleUI.SetState(currentTarget.canFly);
        hintSwimToggleUI.SetState(currentTarget.canSwim);
        hintAttackToggleUI.SetState(currentTarget.canAttack);
    }

    private void UpdateStreakUI()
    {
        if (streak1UI != null) streak1UI.SetState(currentStreak >= 0);
        if (streak2UI != null) streak2UI.SetState(currentStreak >= 1);
        if (streak3UI != null) streak3UI.SetState(currentStreak >= 2);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void CheckPlayerInput(string toggleType, bool newValue)
    {
        if (!isPlaying) return;

        PlaySFX(sfxToggleClick);

        bool isMistake = false;

        if (toggleType == "Fly" && newValue != currentTarget.canFly) isMistake = true;
        if (toggleType == "Swim" && newValue != currentTarget.canSwim) isMistake = true;
        if (toggleType == "Attack" && newValue != currentTarget.canAttack) isMistake = true;

        if (isMistake)
        {
            PlaySFX(sfxIncorrect);
            currentStreak = 0;
            UpdateStreakUI();
            return;
        }

        bool currentFly = flyToggleUI.IsOn;
        bool currentSwim = swimToggleUI.IsOn;
        bool currentAttack = attackToggleUI.IsOn;

        if (currentFly == currentTarget.canFly &&
            currentSwim == currentTarget.canSwim &&
            currentAttack == currentTarget.canAttack)
        {
            HandleCorrectMatch();
        }
    }

    private void HandleCorrectMatch()
    {
        currentStreak++;
        UpdateStreakUI();

        if (currentStreak >= requiredStreak)
        {
            GameOver(true); 
        }
        else
        {
            PlaySFX(sfxCorrect);
            PickNextTarget();
        }
    }

    private void GameOver(bool isWin)
    {
        isPlaying = false;

        if (flyToggleUI != null) flyToggleUI.Toggle.interactable = false;
        if (swimToggleUI != null) swimToggleUI.Toggle.interactable = false;
        if (attackToggleUI != null) attackToggleUI.Toggle.interactable = false;

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
}