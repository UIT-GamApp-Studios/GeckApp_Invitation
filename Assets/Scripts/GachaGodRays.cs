using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn script này vào 1 Image PHỦ TOÀN MÀN HÌNH, đặt trong Hierarchy Ở TRÊN GachaScreenDimmer
/// (và dưới 3 lá bài) để tia sáng vàng xoay chồng lên trên lớp tối.
/// Image này KHÔNG cần gán Sprite (để mặc định "None", vẫn hoạt động vì shader tự vẽ theo UV).
/// Dùng chế độ blend cộng (additive) nên chỉ CỘNG THÊM ánh sáng, không che gì phía dưới.
/// </summary>
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
public class GachaGodRays : MonoBehaviour
{
    [Header("Màu & hình dạng tia sáng")]
    [ColorUsage(true, true)]
    [SerializeField] private Color rayColor = new Color(1f, 0.85f, 0.4f, 1f);

    [Range(4, 64)]
    [SerializeField] private float rayCount = 16f;

    [Range(1f, 30f)]
    [SerializeField] private float raySharpness = 8f;

    [Tooltip("Tốc độ xoay (vòng/giây). Số dương = xoay theo chiều kim đồng hồ.")]
    [Range(-2f, 2f)]
    [SerializeField] private float rotationSpeed = 0.06f;

    [Tooltip("Độ sáng nền của tia (chưa tính pulse/boost)")]
    [Range(0f, 3f)]
    [SerializeField] private float intensity = 0.6f;

    [Header("Vùng ảnh hưởng")]
    [Range(0f, 1f)]
    [SerializeField] private float innerRadius = 0.08f;

    [Range(0.1f, 2f)]
    [SerializeField] private float outerRadius = 0.9f;

    [Header("Nhấp nháy nhẹ theo nhịp")]
    [Range(0f, 2f)]
    [SerializeField] private float pulseIntensity = 0.25f;

    [Range(0.1f, 5f)]
    [SerializeField] private float pulseSpeed = 0.6f;

    [Header("Boost (sáng bừng thêm mỗi khi có thẻ xuất hiện)")]
    [Tooltip("Hệ số nhân độ sáng khi PlayRevealPulse() đang chạy")]
    [Range(1f, 5f)]
    [SerializeField] private float revealBoostMultiplier = 1.8f;

    [Tooltip("Thời gian (giây) để độ sáng dịu dần về mức nền sau mỗi lần PlayRevealPulse()")]
    [Range(0.05f, 3f)]
    [SerializeField] private float revealSettleDuration = 0.6f;

    private Image image;
    private RectTransform rect;
    private Material runtimeMaterial;
    private Coroutine fadeRoutine;
    private Coroutine boostRoutine;
    private float boostMultiplier = 1f;

    private static readonly int RayColorID = Shader.PropertyToID("_RayColor");
    private static readonly int RayCountID = Shader.PropertyToID("_RayCount");
    private static readonly int RaySharpnessID = Shader.PropertyToID("_RaySharpness");
    private static readonly int RotationSpeedID = Shader.PropertyToID("_RotationSpeed");
    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int CenterXID = Shader.PropertyToID("_CenterX");
    private static readonly int CenterYID = Shader.PropertyToID("_CenterY");
    private static readonly int InnerRadiusID = Shader.PropertyToID("_InnerRadius");
    private static readonly int OuterRadiusID = Shader.PropertyToID("_OuterRadius");
    private static readonly int AspectID = Shader.PropertyToID("_Aspect");
    private static readonly int PulseIntensityID = Shader.PropertyToID("_PulseIntensity");
    private static readonly int PulseSpeedID = Shader.PropertyToID("_PulseSpeed");
    private static readonly int BoostID = Shader.PropertyToID("_Boost");

    private void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        image.raycastTarget = false;
        EnsureRuntimeMaterial();
        SetAlpha(0f); // bắt đầu tắt (ẩn), FadeIn() khi cần
    }

    private void EnsureRuntimeMaterial()
    {
        Shader shader = Shader.Find("UI/GachaGodRays");
        if (shader == null)
        {
            Debug.LogWarning("Không tìm thấy shader 'UI/GachaGodRays'. Kiểm tra lại file GachaGodRays.shader đã có trong project.");
            return;
        }

        runtimeMaterial = new Material(shader);
        image.material = runtimeMaterial;
    }

    private void Update()
    {
        if (runtimeMaterial == null) return;

        // Tâm luôn để giữa màn hình (0.5, 0.5) theo UV
        runtimeMaterial.SetColor(RayColorID, rayColor);
        runtimeMaterial.SetFloat(RayCountID, rayCount);
        runtimeMaterial.SetFloat(RaySharpnessID, raySharpness);
        runtimeMaterial.SetFloat(RotationSpeedID, rotationSpeed);
        runtimeMaterial.SetFloat(IntensityID, intensity);
        runtimeMaterial.SetFloat(CenterXID, 0.5f);
        runtimeMaterial.SetFloat(CenterYID, 0.5f);
        runtimeMaterial.SetFloat(InnerRadiusID, innerRadius);
        runtimeMaterial.SetFloat(OuterRadiusID, outerRadius);
        runtimeMaterial.SetFloat(PulseIntensityID, pulseIntensity);
        runtimeMaterial.SetFloat(PulseSpeedID, pulseSpeed);
        runtimeMaterial.SetFloat(BoostID, boostMultiplier);

        // Tự tính tỉ lệ khung hình để tia sáng tròn đều, không bị méo theo Rect
        float w = rect.rect.width;
        float h = rect.rect.height;
        runtimeMaterial.SetFloat(AspectID, h > 0.01f ? w / h : 1f);
    }

    /// <summary>Bật tia sáng lên dần trong "duration" giây (gọi cùng lúc với ScreenDimmer.FadeIn).</summary>
    public void FadeIn(float duration)
    {
        StartFade(1f, duration);
    }

    /// <summary>Tắt tia sáng dần trong "duration" giây (gọi cùng lúc với ScreenDimmer.FadeOut).</summary>
    public void FadeOut(float duration)
    {
        StartFade(0f, duration);
    }

    /// <summary>
    /// Gọi mỗi khi 1 lá bài xuất hiện để tia sáng bừng lên mạnh hơn trong chốc lát rồi dịu lại,
    /// đồng bộ với hiệu ứng ánh sáng vàng trên lá bài đặc biệt.
    /// </summary>
    public void PlayRevealPulse()
    {
        if (boostRoutine != null) StopCoroutine(boostRoutine);
        boostRoutine = StartCoroutine(RevealPulseRoutine());
    }

    private IEnumerator RevealPulseRoutine()
    {
        boostMultiplier = revealBoostMultiplier;
        float t = 0f;
        while (t < revealSettleDuration)
        {
            t += Time.deltaTime;
            boostMultiplier = Mathf.Lerp(revealBoostMultiplier, 1f, t / revealSettleDuration);
            yield return null;
        }
        boostMultiplier = 1f;
        boostRoutine = null;
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
        Color c = image.color;
        c.a = a;
        image.color = c;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            if (Application.isPlaying) Destroy(runtimeMaterial);
            else DestroyImmediate(runtimeMaterial);
        }
    }
}
