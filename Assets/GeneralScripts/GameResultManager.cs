using System;
using UnityEngine;
using UnityEngine.UI;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance { get; private set; }

    public static event Action OnPlayAgainRequested;

    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Win Settings")]
    [SerializeField] private WinPopupAnimation winAnimation;
    [SerializeField] private Button winOverlayButton;

    [Header("Lose Settings")]
    [SerializeField] private Button losePlayAgainButton;
    [SerializeField] private Button loseMenuButton;

    private bool canClickWinToMenu = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (losePlayAgainButton != null)
            losePlayAgainButton.onClick.AddListener(ClickPlayAgain);

        if (loseMenuButton != null)
            loseMenuButton.onClick.AddListener(ReturnToMenu);

        if (winOverlayButton != null)
            winOverlayButton.onClick.AddListener(ReturnToMenu);

        HideAllPanels();
    }

    private void ClickPlayAgain()
    {
        OnPlayAgainRequested?.Invoke();
    }

    private void ReturnToMenu()
    {
        if (winPanel.activeSelf && !canClickWinToMenu) return;
        
        Debug.Log("[GameResultManager] Return To Menu Clicked!");
    }

    public void HideAllPanels()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        canClickWinToMenu = false;
    }

    public void ShowWin()
    {
        HideAllPanels();
        winPanel.SetActive(true);

        if (winAnimation != null)
        {
            winAnimation.PlayWinAnimation(() => canClickWinToMenu = true);
        }
        else
        {
            canClickWinToMenu = true;
        }
    }

    public void ShowLose()
    {
        HideAllPanels();
        losePanel.SetActive(true);
    }
}