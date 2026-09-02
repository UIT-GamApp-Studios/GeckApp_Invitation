    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

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

        [Header("Scene Settings")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private bool canClickWinToMenu = false;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupCanvasSorting();
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

        private void SetupCanvasSorting()
        {
            Canvas canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9990; 
            }
        }

        private void ClickPlayAgain()
        {
            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.PlayTransition(() =>
                {
                    Time.timeScale = 1f;
                    HideAllPanels();
                    
                    Scene activeScene = SceneManager.GetActiveScene();
                    SceneManager.LoadScene(activeScene.name);

                    OnPlayAgainRequested?.Invoke();
                });
            }
            else
            {
                Time.timeScale = 1f;
                HideAllPanels();
                OnPlayAgainRequested?.Invoke();
            }
        }

        private void ReturnToMenu()
        {
            if (winPanel != null && winPanel.activeSelf && !canClickWinToMenu) return;

            if (SceneController.Instance != null)
            {
                SceneController.Instance.ChangeScene(mainMenuSceneName);
                HideAllPanels();
            }
            else if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.PlayTransition(() =>
                {
                    Time.timeScale = 1f;
                    HideAllPanels();
                    SceneManager.LoadScene(mainMenuSceneName);
                });
            }
            else
            {
                Time.timeScale = 1f;
                HideAllPanels();
                SceneManager.LoadScene(mainMenuSceneName);
            }
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

            if (winPanel != null) winPanel.SetActive(true);

            string currentSceneName = SceneManager.GetActiveScene().name;
            if (GachaRewardManager.Instance != null)
            {
                GachaRewardManager.Instance.EarnTokenForScene(currentSceneName);
            }

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
            if (losePanel != null) losePanel.SetActive(true);
        }
    }