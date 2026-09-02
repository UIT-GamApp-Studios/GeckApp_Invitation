using UnityEngine;
using TMPro;
using DG.Tweening;

public class PhotoFeedbackUI : MonoBehaviour
{
    [SerializeField] private MediaGameConfig config;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Animation Dynamics")]
    [SerializeField] private float floatDistance = 90f;
    [SerializeField] private float popScale = 1.35f;
    [SerializeField] private float popDuration = 0.22f;
    [SerializeField] private float holdDuration = 0.3f;
    [SerializeField] private float fadeDuration = 0.18f;

    private RectTransform rectTransform;
    private Vector2 baseAnchoredPos;
    private Sequence currentSequence;

    [Header("Audio Feedback Setup")]
    [SerializeField] private SoundFeedback perfectSound;
    [SerializeField] private SoundFeedback goodSound;
    [SerializeField] private SoundFeedback missSound;

    private void Awake()
    {
        if (feedbackText != null)
        {
            rectTransform = feedbackText.rectTransform;
            baseAnchoredPos = rectTransform.anchoredPosition;
            feedbackText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPhotoTaken += ShowFeedback;
    }

    private void OnDisable()
    {
        GameEvents.OnPhotoTaken -= ShowFeedback;
    }

    private void ShowFeedback(PhotoResult result, int scoreAdded, int currentScore)
    {
        if (feedbackText == null || config == null) return;

        currentSequence?.Kill(true);

        switch (result)
        {
            case PhotoResult.Perfect:
                feedbackText.text = config.perfectText;
                feedbackText.color = config.perfectColor;
                if (perfectSound != null) perfectSound.PlaySound();
                break;

            case PhotoResult.Good:
                feedbackText.text = config.goodText;
                feedbackText.color = config.goodColor;
                if (goodSound != null) goodSound.PlaySound();
                break;

            case PhotoResult.Miss:
                feedbackText.text = config.missText;
                feedbackText.color = config.missColor;
                Debug.Log("Play");
                if (missSound != null) missSound.PlaySound();
                break;
        }

        rectTransform.anchoredPosition = baseAnchoredPos;
        rectTransform.localScale = Vector3.zero;
        
        Color baseColor = feedbackText.color;
        baseColor.a = 1f;
        feedbackText.color = baseColor;

        feedbackText.gameObject.SetActive(true);

        float targetY = baseAnchoredPos.y + floatDistance;

        currentSequence = DOTween.Sequence()
            .Append(rectTransform.DOAnchorPosY(targetY, popDuration).SetEase(Ease.OutCubic))
            .Join(rectTransform.DOScale(popScale, popDuration).SetEase(Ease.OutBack))
            .AppendInterval(holdDuration)
            .Append(feedbackText.DOFade(0f, fadeDuration).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                rectTransform.anchoredPosition = baseAnchoredPos;
                feedbackText.gameObject.SetActive(false);
            });
    }

    private void OnDestroy()
    {
        currentSequence?.Kill();
    }
}