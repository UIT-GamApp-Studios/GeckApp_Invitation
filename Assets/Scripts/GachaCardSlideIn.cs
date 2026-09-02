using System.Collections;
using UnityEngine;

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
    private bool isInitialized = false;

    public float SlideDuration => slideDuration;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (isInitialized) return;
        rect = GetComponent<RectTransform>();
        targetPos = rect.anchoredPosition; // Lưu lại vị trí chuẩn đặt trong Editor
        isInitialized = true;
    }

    private void OnEnable()
    {
        Init();
        // Đẩy lá bài lên trên ngoài khung hình để chuẩn bị trượt (khi gọi Play)
        rect.anchoredPosition = targetPos + new Vector2(0f, startOffsetY);
    }

    public void Play(float delay = 0f)
    {
        StopAllCoroutines();
        StartCoroutine(SlideRoutine(delay));
    }

    // HÀM MỚI: Nhảy thẳng về vị trí đích (dành cho lần vào Gacha thứ 2 trở đi)
    public void SnapToFinalPosition()
    {
        Init();
        StopAllCoroutines();
        gameObject.SetActive(true);
        rect.anchoredPosition = targetPos; // Ép lá bài về đúng vị trí đích ngay lập tức
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