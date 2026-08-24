using UnityEngine;
using TMPro;
using DG.Tweening;

public class ScoreFeedbackUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Punch Effect Settings")]
    [SerializeField] private Vector3 punchScale = new Vector3(0.35f, 0.35f, 0f);
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private int vibrato = 5;
    [SerializeField] private float elasticity = 0.5f;

    private Tween punchTween;

    private void OnEnable()
    {
        GameEvents.OnPhotoTaken += HandlePhotoTaken;
    }

    private void OnDisable()
    {
        GameEvents.OnPhotoTaken -= HandlePhotoTaken;
    }

    private void HandlePhotoTaken(PhotoResult result, int scoreAdded, int currentScore)
    {
        if (scoreText == null) return;

        scoreText.text = currentScore.ToString();

        if (result == PhotoResult.Perfect)
        {
            punchTween?.Kill(true);
            scoreText.transform.localScale = Vector3.one;

            punchTween = scoreText.transform
                .DOPunchScale(punchScale, duration, vibrato, elasticity)
                .OnKill(() => scoreText.transform.localScale = Vector3.one);
        }
    }

    private void OnDestroy()
    {
        punchTween?.Kill();
    }
}