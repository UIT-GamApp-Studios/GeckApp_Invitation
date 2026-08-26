using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GeckApp.UI
{
    /// <summary>
    /// Glassmorphism setting popup. Wire sliders, close button, backdrop button in Inspector.
    /// Hook events (OnCloseRequested / OnExitLevelRequested / OnRestartLevelRequested) from code or other components.
    /// </summary>
    public class SettingPopup : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider bgmSlider;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;
        [SerializeField] private Button exitLevelButton;
        [SerializeField] private Button restartLevelButton;

        [Header("Animation")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private CanvasGroup backdrop;
        [SerializeField] private CanvasGroup root;
        [SerializeField] private float openDuration = 0.22f;
        [SerializeField] private float closeDuration = 0.16f;
        [SerializeField] private Vector3 openScale = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 closedScale = new Vector3(0.86f, 0.86f, 1f);

        public event Action OnCloseRequested;
        public event Action OnExitLevelRequested;
        public event Action OnRestartLevelRequested;

        private const string SFX_KEY = "Settings.SfxVolume";
        private const string BGM_KEY = "Settings.BgmVolume";

        private Coroutine _activeAnim;

        private void Awake()
        {
            if (root == null) root = GetComponent<CanvasGroup>();
            if (root != null)
            {
                root.alpha = 0f;
                root.interactable = false;
                root.blocksRaycasts = false;
            }
            gameObject.SetActive(false);

            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBgmChanged);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (backdropButton != null) backdropButton.onClick.AddListener(Close);
            if (exitLevelButton != null) exitLevelButton.onClick.AddListener(() => OnExitLevelRequested?.Invoke());
            if (restartLevelButton != null) restartLevelButton.onClick.AddListener(() => OnRestartLevelRequested?.Invoke());

            LoadInitialValues();
        }

        public void Open()
        {
            gameObject.SetActive(true);
            if (_activeAnim != null) StopCoroutine(_activeAnim);
            _activeAnim = StartCoroutine(AnimateOpen());
        }

        public void Close()
        {
            if (_activeAnim != null) StopCoroutine(_activeAnim);
            _activeAnim = StartCoroutine(AnimateClose());
        }

        private IEnumerator AnimateOpen()
        {
            float t = 0f;
            if (root != null) { root.interactable = false; root.blocksRaycasts = true; }
            if (backdrop != null) backdrop.alpha = 0f;
            if (panel != null) panel.localScale = closedScale;

            while (t < openDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / openDuration);
                float e = 1f - Mathf.Pow(1f - k, 3f);
                if (root != null) root.alpha = e;
                if (backdrop != null) backdrop.alpha = e * 0.7f;
                if (panel != null)
                {
                    Vector3 s = Vector3.LerpUnclamped(closedScale, openScale, e);
                    panel.localScale = s;
                }
                yield return null;
            }

            if (root != null) { root.alpha = 1f; root.interactable = true; }
            if (backdrop != null) backdrop.alpha = 0.7f;
            if (panel != null) panel.localScale = openScale;
            _activeAnim = null;
        }

        private IEnumerator AnimateClose()
        {
            float t = 0f;
            if (root != null) root.interactable = false;
            float startAlpha = root != null ? root.alpha : 1f;
            float startBackdropAlpha = backdrop != null ? backdrop.alpha : 0.7f;
            Vector3 startScale = panel != null ? panel.localScale : openScale;

            while (t < closeDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / closeDuration);
                float e = k * k * k;
                if (root != null) root.alpha = Mathf.Lerp(startAlpha, 0f, e);
                if (backdrop != null) backdrop.alpha = Mathf.Lerp(startBackdropAlpha, 0f, e);
                if (panel != null)
                {
                    Vector3 s = Vector3.LerpUnclamped(startScale, closedScale, e);
                    panel.localScale = s;
                }
                yield return null;
            }

            if (root != null) { root.alpha = 0f; root.blocksRaycasts = false; }
            if (backdrop != null) backdrop.alpha = 0f;
            if (panel != null) panel.localScale = closedScale;
            gameObject.SetActive(false);
            OnCloseRequested?.Invoke();
            _activeAnim = null;
        }

        private void LoadInitialValues()
        {
            float sfx = PlayerPrefs.GetFloat(SFX_KEY, 1f);
            float bgm = PlayerPrefs.GetFloat(BGM_KEY, 1f);
            if (sfxSlider != null) { sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f; sfxSlider.SetValueWithoutNotify(sfx); }
            if (bgmSlider != null) { bgmSlider.minValue = 0f; bgmSlider.maxValue = 1f; bgmSlider.SetValueWithoutNotify(bgm); }
            AudioListener.volume = bgm;
        }

        private void OnSfxChanged(float value)
        {
            PlayerPrefs.SetFloat(SFX_KEY, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }

        private void OnBgmChanged(float value)
        {
            float v = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(BGM_KEY, v);
            PlayerPrefs.Save();
            AudioListener.volume = v;
        }
    }
}