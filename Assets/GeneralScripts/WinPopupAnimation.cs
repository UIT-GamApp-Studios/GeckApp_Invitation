using UnityEngine;
using DG.Tweening;

public class WinPopupAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float overshoot = 1.2f;
    [SerializeField] private Vector3 punchScale = new Vector3(0.15f, 0.15f, 0f);

    private RectTransform rectTransform;
    private Tween currentTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void PlayWinAnimation(System.Action onComplete = null)
    {
        currentTween?.Kill();

        rectTransform.localScale = Vector3.zero;

        Sequence winSequence = DOTween.Sequence();

        winSequence.Append(
            rectTransform.DOScale(Vector3.one, duration)
                .SetEase(Ease.OutBack, overshoot)
        );

        winSequence.Append(
            rectTransform.DOPunchScale(punchScale, 0.3f, 5, 0.5f)
        );

        winSequence.SetUpdate(true);
        winSequence.OnComplete(() => onComplete?.Invoke());

        currentTween = winSequence;
    }

    private void OnDisable()
    {
        currentTween?.Kill();
    }
}