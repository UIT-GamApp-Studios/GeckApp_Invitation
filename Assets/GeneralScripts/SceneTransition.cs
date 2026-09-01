using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image flashImage;
    private Canvas transitionCanvas;

    [Header("Settings")]
    [SerializeField] private int sortingOrder = 9999;
    [SerializeField] private Color flashColor = Color.black;
    [SerializeField] private float fadeInTime = 0.25f;
    [SerializeField] private float fadeOutTime = 0.25f;
    [SerializeField] private float waitTime = 0.2f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupCanvas();
    }

    private void SetupCanvas()
    {
        transitionCanvas = GetComponent<Canvas>();
        
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = sortingOrder;

        if (flashImage != null)
        {
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            flashImage.raycastTarget = false;
            flashImage.gameObject.SetActive(true);
        }
    }

    public void PlayTransition(Action onMidFlash = null)
    {
        if (isTransitioning || flashImage == null) return;
        isTransitioning = true;

        flashImage.raycastTarget = true;

        Sequence seq = DOTween.Sequence();

        seq.SetUpdate(true);
        
        seq.Append(flashImage.DOFade(1f, fadeInTime));
        seq.AppendCallback(() => 
        { 
            onMidFlash?.Invoke(); 
        });
        seq.AppendInterval(waitTime);
        seq.Append(flashImage.DOFade(0f, fadeOutTime));
        seq.OnComplete(() => 
        { 
            isTransitioning = false;
            flashImage.raycastTarget = false;
        });
    }
}