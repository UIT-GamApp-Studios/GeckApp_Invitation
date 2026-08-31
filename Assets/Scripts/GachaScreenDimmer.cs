using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn script này vào 1 Image PHỦ TOÀN MÀN HÌNH, đặt trong Hierarchy ở TRÊN Background
/// nhưng ở DƯỚI 3 lá bài (CardSlot1/2/3) để lá bài luôn nổi bật trên nền tối.
/// Image này để màu đen, alpha ban đầu = 0 (trong suốt) — script sẽ tự điều khiển alpha.
/// Gọi FadeIn() để làm tối màn hình, FadeOut() để trả lại bình thường.
/// </summary>
[RequireComponent(typeof(Image))]
public class GachaScreenDimmer : MonoBehaviour
{
    [Tooltip("Độ tối tối đa khi FadeIn xong (0 = trong suốt, 1 = đen hoàn toàn)")]
    [Range(0f, 1f)]
    [SerializeField] private float maxDimAlpha = 0.65f;

    [Tooltip("Màu lớp phủ tối (thường để đen)")]
    [SerializeField] private Color dimColor = Color.black;

    private Image image;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        image = GetComponent<Image>();
        // Không chặn thao tác chạm/click của các UI khác (nút back, v.v.)
        image.raycastTarget = false;
        SetAlpha(0f);
    }

    /// <summary>Làm tối màn hình dần trong "duration" giây.</summary>
    public void FadeIn(float duration)
    {
        StartFade(maxDimAlpha, duration);
    }

    /// <summary>Trả màn hình về bình thường (hết tối) dần trong "duration" giây.</summary>
    public void FadeOut(float duration)
    {
        StartFade(0f, duration);
    }

    private void StartFade(float targetAlpha, float duration)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = image.color.a;

        if (duration <= 0f)
        {
            SetAlpha(targetAlpha);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(t / duration)));
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float a)
    {
        Color c = dimColor;
        c.a = a;
        image.color = c;
    }
}
