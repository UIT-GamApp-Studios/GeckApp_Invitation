using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class LogoLightEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [ColorUsage(true, true)]
    public Color glowColor = new Color(0f, 1f, 1f, 1f);
    [Range(0f, 5f)]
    public float glowIntensity = 0.6f;
    [Range(0.1f, 5f)]
    public float glowPower = 1.5f;
    [Range(0f, 0.2f)]
    public float glowSize = 0.012f;
    
    [Header("Sweep Settings")]
    [ColorUsage(true, true)]
    public Color sweepColor = new Color(1f, 1f, 1f, 1f);
    [ColorUsage(true, true)]
    public Color sweepHighlight = new Color(1f, 0.95f, 0.9f, 1f);
    [Range(0.05f, 1f)]
    public float sweepWidth = 0.08f;
    [Range(0f, 5f)]
    public float sweepIntensity = 1.5f;
    [Range(0.01f, 0.5f)]
    public float sweepSoftness = 0.04f;
    [Range(0.1f, 5f)]
    public float sweepSpeed = 0.8f;
    [Tooltip("0 = chạy ngang trái sang phải (mặc định, dùng cho logo). 1 = chạy chéo từ góc dưới trái lên góc trên phải (dùng cho thẻ đặc biệt).")]
    [Range(0f, 1f)]
    public float sweepDirection = 0f;
    
    [Header("Pulse Settings")]
    [Range(0f, 2f)]
    public float pulseIntensity = 0.15f;
    [Range(0.1f, 5f)]
    public float pulseSpeed = 1.0f;
    
    [Header("Secondary Accent")]
    [ColorUsage(true, true)]
    public Color secondaryColor = new Color(1f, 0.6f, 0.3f, 1f);
    [Range(0f, 2f)]
    public float secondaryIntensity = 0.25f;

    [Header("Reveal Boost (bùng sáng lúc thẻ đang trượt xuống)")]
    [Tooltip("Hệ số nhân độ sáng/độ quét trong lúc PlayRevealBoost() đang chạy, " +
             "giúp ánh sáng vàng RÕ hơn hẳn ngay khi thẻ xuất hiện.")]
    [Range(1f, 5f)]
    public float revealBoostMultiplier = 2.2f;

    [Tooltip("Thời gian (giây) để độ sáng dịu dần từ mức boost về mức bình thường sau khi thẻ trượt xong")]
    [Range(0.05f, 2f)]
    public float revealSettleDuration = 0.35f;

    private float boostMultiplier = 1f;
    private Coroutine boostRoutine;

    private Image image;
    private Material runtimeMaterial;
    private static readonly int GlowColorID = Shader.PropertyToID("_GlowColor");
    private static readonly int GlowIntensityID = Shader.PropertyToID("_GlowIntensity");
    private static readonly int GlowPowerID = Shader.PropertyToID("_GlowPower");
    private static readonly int GlowSizeID = Shader.PropertyToID("_GlowSize");
    private static readonly int SweepColorID = Shader.PropertyToID("_SweepColor");
    private static readonly int SweepHighlightID = Shader.PropertyToID("_SweepHighlight");
    private static readonly int SweepWidthID = Shader.PropertyToID("_SweepWidth");
    private static readonly int SweepIntensityID = Shader.PropertyToID("_SweepIntensity");
    private static readonly int SweepSoftnessID = Shader.PropertyToID("_SweepSoftness");
    private static readonly int SweepSpeedID = Shader.PropertyToID("_SweepSpeed");
    private static readonly int SweepDirectionID = Shader.PropertyToID("_SweepDirection");
    private static readonly int PulseIntensityID = Shader.PropertyToID("_PulseIntensity");
    private static readonly int PulseSpeedID = Shader.PropertyToID("_PulseSpeed");
    private static readonly int SecondaryColorID = Shader.PropertyToID("_SecondaryColor");
    private static readonly int SecondaryIntensityID = Shader.PropertyToID("_SecondaryIntensity");
    
    void OnEnable()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            EnsureRuntimeMaterial();
        }
    }
    
    void EnsureRuntimeMaterial()
    {
        Shader shader = Shader.Find("UI/LogoLightEffect");
        if (shader == null)
        {
            Debug.LogWarning("LogoLightEffect shader not found. Make sure 'UI/LogoLightEffect' shader is included in the project.");
            return;
        }
        
        if (image.material == null || image.material.shader == null || image.material.shader.name != "UI/LogoLightEffect")
        {
            runtimeMaterial = new Material(shader);
            image.material = runtimeMaterial;
        }
        else
        {
            runtimeMaterial = image.material;
        }
    }
    
    void Update()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
            if (image == null) return;
        }
        
        if (runtimeMaterial == null)
        {
            EnsureRuntimeMaterial();
            if (runtimeMaterial == null) return;
        }
        
        ApplyMaterialProperties();
    }
    
    void ApplyMaterialProperties()
    {
        if (runtimeMaterial == null) return;
        
        runtimeMaterial.SetColor(GlowColorID, glowColor);
        runtimeMaterial.SetFloat(GlowIntensityID, glowIntensity * boostMultiplier);
        runtimeMaterial.SetFloat(GlowPowerID, glowPower);
        runtimeMaterial.SetFloat(GlowSizeID, glowSize);
        
        runtimeMaterial.SetColor(SweepColorID, sweepColor);
        runtimeMaterial.SetColor(SweepHighlightID, sweepHighlight);
        runtimeMaterial.SetFloat(SweepWidthID, sweepWidth);
        runtimeMaterial.SetFloat(SweepIntensityID, sweepIntensity * boostMultiplier);
        runtimeMaterial.SetFloat(SweepSoftnessID, sweepSoftness);
        runtimeMaterial.SetFloat(SweepSpeedID, sweepSpeed);
        runtimeMaterial.SetFloat(SweepDirectionID, sweepDirection);
        
        runtimeMaterial.SetFloat(PulseIntensityID, pulseIntensity);
        runtimeMaterial.SetFloat(PulseSpeedID, pulseSpeed);
        
        runtimeMaterial.SetColor(SecondaryColorID, secondaryColor);
        runtimeMaterial.SetFloat(SecondaryIntensityID, secondaryIntensity * boostMultiplier);
    }

    /// <summary>
    /// Gọi ngay khi lá bài BẮT ĐẦU trượt xuống (vd: cùng lúc gọi cardSlides[i].Play()).
    /// Trong suốt "duration" giây (thường = SlideDuration của lá bài), ánh sáng vàng sẽ
    /// RÕ HƠN HẲN (sáng + quét mạnh hơn revealBoostMultiplier lần), sau đó dịu dần về mức
    /// bình thường trong revealSettleDuration giây khi lá bài đã nằm yên.
    /// </summary>
    public void PlayRevealBoost(float duration)
    {
        if (boostRoutine != null) StopCoroutine(boostRoutine);
        boostRoutine = StartCoroutine(RevealBoostRoutine(duration));
    }

    private IEnumerator RevealBoostRoutine(float duration)
    {
        // Pha 1: giữ độ sáng cao trong lúc thẻ đang trượt xuống
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            boostMultiplier = revealBoostMultiplier;
            yield return null;
        }

        // Pha 2: hạ dần về mức bình thường sau khi thẻ đã nằm yên
        t = 0f;
        while (t < revealSettleDuration)
        {
            t += Time.deltaTime;
            boostMultiplier = Mathf.Lerp(revealBoostMultiplier, 1f, t / revealSettleDuration);
            yield return null;
        }

        boostMultiplier = 1f;
        boostRoutine = null;
    }
    
    void OnDisable()
    {
        boostMultiplier = 1f;
        if (boostRoutine != null)
        {
            StopCoroutine(boostRoutine);
            boostRoutine = null;
        }

        // Cho phép bật/tắt component này (vd: chỉ bật khi thẻ đặc biệt xuất hiện)
        // mà không để lại material cũ dính trên Image.
        if (image != null && runtimeMaterial != null && image.material == runtimeMaterial)
        {
            image.material = null;
        }
    }
    
    void OnDestroy()
    {
        if (runtimeMaterial != null && Application.isPlaying)
        {
            if (Application.isPlaying)
            {
                Destroy(runtimeMaterial);
            }
            else
            {
                DestroyImmediate(runtimeMaterial);
            }
        }
    }
}