using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

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
    [SerializeField] private MinigameToggleUI streak1UI; // Maps to Streak1
    [SerializeField] private MinigameToggleUI streak2UI; // Maps to Streak2
    [SerializeField] private MinigameToggleUI streak3UI; // Maps to Streak3

    [Header("UI Visuals")]
    [SerializeField] private Image objectiveImage; 
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Settings & Database")]
    [SerializeField] private float timeLimit = 15f; // Matches the GDD spec[cite: 1]
    [SerializeField] private int requiredStreak = 3;
    [SerializeField] private List<ObjectDefinition> objectDatabase;

    public UnityEvent OnTokenEarned;

    private ObjectDefinition currentTarget;
    private float currentTime;
    private int currentStreak = 0;
    private bool isPlaying = false;
    private bool hasEarnedToken = false;
    private int lastTargetIndex = -1;

    // We leave Awake empty or remove it entirely. 
    // By moving setup to Start(), we guarantee MinigameToggleUI has already found its Toggle components.
    private void Start()
    {
        // 1. Lock hint toggles...
        if (hintFlyToggleUI != null && hintFlyToggleUI.Toggle != null) hintFlyToggleUI.Toggle.interactable = false;
        if (hintSwimToggleUI != null && hintSwimToggleUI.Toggle != null) hintSwimToggleUI.Toggle.interactable = false;
        if (hintAttackToggleUI != null && hintAttackToggleUI.Toggle != null) hintAttackToggleUI.Toggle.interactable = false;

        // 2. Lock streak indicators individually...
        if (streak1UI != null && streak1UI.Toggle != null) streak1UI.Toggle.interactable = false;
        if (streak2UI != null && streak2UI.Toggle != null) streak2UI.Toggle.interactable = false;
        if (streak3UI != null && streak3UI.Toggle != null) streak3UI.Toggle.interactable = false;

        // 3. NEW: Bind Gameplay listeners securely with context!
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

        // 1. Declare our random boolean variables
        bool randFly;
        bool randSwim;
        bool randAttack;

        // 2. NEW LOGIC: Keep scrambling the toggles until they do NOT 
        // perfectly match the target. This guarantees the player must 
        // make at least one move to solve the puzzle.
        do
        {
            randFly = Random.value > 0.5f;
            randSwim = Random.value > 0.5f;
            randAttack = Random.value > 0.5f;
        } 
        while (randFly == currentTarget.canFly && 
               randSwim == currentTarget.canSwim && 
               randAttack == currentTarget.canAttack);

        // 3. Apply the guaranteed-scrambled state to the UI silently
        flyToggleUI.SetState(randFly);
        swimToggleUI.SetState(randSwim);
        attackToggleUI.SetState(randAttack);

        // 4. REMOVE the old "Silent Check" entirely. We no longer need 
        // to check for an accidental win, because the 'do-while' loop 
        // makes an accidental win mathematically impossible.
    }

    private void UpdateHints()
    {
        hintFlyToggleUI.SetState(currentTarget.canFly);
        hintSwimToggleUI.SetState(currentTarget.canSwim);
        hintAttackToggleUI.SetState(currentTarget.canAttack);
    }

    private void UpdateStreakUI()
    {
        // currentStreak = 0 -> Streak1 ON
        // currentStreak = 1 -> Streak1 & 2 ON
        // currentStreak = 2 -> Streak1, 2, & 3 ON
        if (streak1UI != null) streak1UI.SetState(currentStreak >= 0);
        if (streak2UI != null) streak2UI.SetState(currentStreak >= 1);
        if (streak3UI != null) streak3UI.SetState(currentStreak >= 2);
    }

    private void CheckPlayerInput(string toggleType, bool newValue)
    {
        if (!isPlaying) return;

        // 1. Evaluate if the SPECIFIC switch the player just clicked is wrong
        bool isMistake = false;

        if (toggleType == "Fly" && newValue != currentTarget.canFly) isMistake = true;
        if (toggleType == "Swim" && newValue != currentTarget.canSwim) isMistake = true;
        if (toggleType == "Attack" && newValue != currentTarget.canAttack) isMistake = true;

        if (isMistake)
        {
            Debug.Log($"Mistake made on {toggleType} (Set to {newValue}). Resetting streak to 0!");
            currentStreak = 0; // Enforces the GDD "Wrong -> Reset the streak" rule
            UpdateStreakUI();
            return; // Stop checking for a win since they made a mistake
        }

        // 2. If it wasn't a mistake, check if ALL switches now perfectly match the target
        bool currentFly = flyToggleUI.IsOn;
        bool currentSwim = swimToggleUI.IsOn;
        bool currentAttack = attackToggleUI.IsOn;

        if (currentFly == currentTarget.canFly &&
            currentSwim == currentTarget.canSwim &&
            currentAttack == currentTarget.canAttack)
        {
            Debug.Log("Result: All attributes perfectly match!");
            HandleCorrectMatch();
        }
    }

    private void HandleCorrectMatch()
    {
        currentStreak++;
        Debug.Log($"Streak Incremented! New Streak: {currentStreak}");
        UpdateStreakUI();

        if (currentStreak >= requiredStreak)
        {
            Debug.Log("Win Condition Met! Firing GameOver.");
            GameOver(true);
        }
        else
        {
            PickNextTarget();
        }
    }

    private void GameOver(bool isWin)
    {
        isPlaying = false;

        if (isWin && !hasEarnedToken)
        {
            hasEarnedToken = true;
            Debug.Log("Token earned");
            OnTokenEarned?.Invoke();
        }
    }
}