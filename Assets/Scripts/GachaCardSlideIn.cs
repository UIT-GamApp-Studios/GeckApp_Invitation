using System.Collections;
using UnityEngine;

/// <summary>
/// Gắn script này vào từng lá bài (Image) trong scene Gacha.
/// Đặt RectTransform của lá bài ở đúng VỊ TRÍ ĐÍCH mong muốn ngay trong Editor;
/// khi chạy, script tự đẩy lá bài lên trên khung hình rồi trượt xuống vị trí đó.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GachaCardSlideIn : MonoBehaviour
{
    [Tooltip("Thời gian trượt xuống (giây)")]
    [SerializeField] private float slideDuration = 0.5f;

    [Tooltip("Khoảng cách phía trên màn hình để lá bài bắt đầu trượt từ đó xuống")]
    [SerializeField] private float startOffsetY = 1000f;

    [Tooltip("Đường cong easing cho chuyển động (mặc định ease-out nhẹ)")]
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rect;
    private Vector2 targetPos;

    /// <summary>Thời gian trượt xuống (giây), dùng để GachaManager biết khi nào thẻ này trượt xong.</summary>
    public float SlideDuration => slideDuration;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        // Vị trí đích = vị trí bạn đã đặt sẵn cho lá bài trong Editor
        targetPos = rect.anchoredPosition;
    }

    private void OnEnable()
    {
        // Đẩy lá bài lên trên, ngoài khung hình, chờ lệnh Play() để trượt xuống
        rect.anchoredPosition = targetPos + new Vector2(0f, startOffsetY);
    }

    public void Play(float delay = 0f)
    {
        StopAllCoroutines();
        StartCoroutine(SlideRoutine(delay));
    }

    private IEnumerator SlideRoutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        Vector2 startPos = targetPos + new Vector2(0f, startOffsetY);
        float t = 0f;

        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float progress = easeCurve.Evaluate(Mathf.Clamp01(t / slideDuration));
            rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, progress);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }
}
