using System.Collections;
using UnityEngine;

public class MediaGameManager : MonoBehaviour
{
    [SerializeField] private MediaGameConfig config;
    [SerializeField] private LensController lens;
    [SerializeField] private MascotSpawner spawner;

    [Header("End Game Delay")]
    [Tooltip("Khoảng thời gian chờ sau khi đồng hồ về 0 rồi mới PauseGame")]
    [SerializeField] private float gameOverDelay = 1.0f;

    public int CurrentScore { get; private set; } = 0;
    public float RemainingTime { get; private set; }
    public bool IsGameActive { get; private set; } = false;

    private bool hasTriggeredWarning = false;

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!IsGameActive) return;

        RemainingTime -= Time.deltaTime;

        if (!hasTriggeredWarning && RemainingTime <= config.timeWarningThreshold)
        {
            hasTriggeredWarning = true;
            GameEvents.OnTimeWarning?.Invoke();
        }

        if (RemainingTime <= 0)
        {
            RemainingTime = 0;
            EndGame(false);
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1.0f;
        CurrentScore = 0;
        RemainingTime = config.timeLimit;
        IsGameActive = true;
        hasTriggeredWarning = false;

        spawner.StartSpawning();
    }

    public void TakePhoto()
    {
        if (!IsGameActive) return;

        PhotoResult bestResult = PhotoResult.Miss;
        int scoreAddedThisShot = 0;

        Vector2 lensCenter = lens.transform.position;

        foreach (var mascot in spawner.ActiveMascots)
        {
            if (mascot == null) continue;

            PhotoResult result = mascot.EvaluatePhotoQuality(lensCenter, config.lensRadius);

            if (result == PhotoResult.Perfect)
            {
                bestResult = PhotoResult.Perfect;

                if (!mascot.IsCaptured)
                {
                    mascot.MarkAsCaptured();
                    scoreAddedThisShot += 1;
                }
            }
            else if (result == PhotoResult.Good && bestResult != PhotoResult.Perfect)
            {
                bestResult = PhotoResult.Good;
            }
        }

        if (scoreAddedThisShot > 0)
        {
            CurrentScore += scoreAddedThisShot;
        }

        GameEvents.OnPhotoTaken?.Invoke(bestResult, scoreAddedThisShot, CurrentScore);

        if (CurrentScore >= config.targetScore)
        {
            EndGame(true);
        }
    }

    private void EndGame(bool isWin)
    {
        IsGameActive = false;
        spawner.StopSpawning();
        GameEvents.OnGameEnd?.Invoke(isWin);

        if (isWin)
        {
            Debug.Log("Win!");
        }
        else
        {
            Debug.Log("Lose!");
        }

        StartCoroutine(PauseGameRoutine());
    }

    private IEnumerator PauseGameRoutine()
    {
        yield return new WaitForSecondsRealtime(gameOverDelay);
        PauseGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Game Paused!");
    }
}