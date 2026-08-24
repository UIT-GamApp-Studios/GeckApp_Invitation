using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MediaUIManager : MonoBehaviour
{
    [SerializeField] private MediaGameManager gameManager;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button photoButton;

    private void OnEnable()
    {
        GameEvents.OnPhotoTaken += HandlePhotoTaken;
        GameEvents.OnGameEnd += HandleGameEnd;
    }

    private void OnDisable()
    {
        GameEvents.OnPhotoTaken -= HandlePhotoTaken;
        GameEvents.OnGameEnd -= HandleGameEnd;
    }

    private void Start()
    {
        if (photoButton != null)
        {
            photoButton.onClick.AddListener(() => gameManager.TakePhoto());
        }

        ResetUI();
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsGameActive)
        {
            UpdateTimerUI();
        }
    }

    public void ResetUI()
    {
        if (gameManager != null)
        {
            if (scoreText != null)
                scoreText.text = $"{gameManager.CurrentScore}/10";

            UpdateTimerUI();
        }

        if (photoButton != null)
        {
            photoButton.interactable = true;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null && gameManager != null)
        {
            int timeToDisplay = Mathf.Max(0, Mathf.FloorToInt(gameManager.RemainingTime));
            timerText.text = $"Time: {timeToDisplay:D2}";
        }
    }

    private void HandlePhotoTaken(PhotoResult result, int scoreAdded, int currentScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{currentScore}/10";
        }
    }

    private void HandleGameEnd(bool isWin)
    {
        if (timerText != null && !isWin)
        {
            timerText.text = "Time: 00";
        }

        if (photoButton != null)
        {
            photoButton.interactable = false;
        }
    }
}