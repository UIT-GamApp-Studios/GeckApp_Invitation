using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LogoIntroAnimation : MonoBehaviour
{
    private static bool isFirstBoot = true;

    [Header("Animation Settings")]
    public float introDelay = 0.2f;
    public float moveDuration = 1.0f;
    public float holdDuration = 0.3f;
    public float mainScreenFadeInDuration = 0.6f;
    
    [Header("Center Position")]
    public Vector2 centerPosition = Vector2.zero;
    public float introScale = 1.4f;
    
    [Header("Easing")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Main Screen Elements")]
    public GameObject[] mainScreenElements;
    
    private RectTransform logoRT;
    private CanvasGroup logoCanvasGroup;
    private Vector2 finalPosition;
    private Vector3 finalScale;
    private bool hasAnimated = false;

    public bool HasFinishedIntro { get; private set; } = false;
    public static bool IsFirstBoot => isFirstBoot; // Cho phép script khác kiểm tra đúng trạng thái
    
    void Awake()
    {
        logoRT = GetComponent<RectTransform>();
        logoCanvasGroup = GetComponent<CanvasGroup>();
        if (logoCanvasGroup == null)
        {
            logoCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        finalPosition = logoRT.anchoredPosition;
        finalScale = logoRT.localScale;
    }
    
    void Start()
    {
        if (isFirstBoot)
        {
            if (!hasAnimated)
            {
                StartCoroutine(PlayIntro());
            }
        }
        else
        {
            SkipIntroImmediate();
        }
    }

    private void SkipIntroImmediate()
    {
        hasAnimated = true;

        logoRT.anchoredPosition = finalPosition;
        logoRT.localScale = finalScale;
        logoCanvasGroup.alpha = 1f;
        logoCanvasGroup.blocksRaycasts = true;

        EnableMainScreen(true);
        SetMainScreenAlpha(1f);
        SetMainScreenInteractable(true);

        HasFinishedIntro = true;
    }
    
    IEnumerator PlayIntro()
    {
        hasAnimated = true;
        
        // 1. Setup initial state
        logoRT.anchoredPosition = centerPosition;
        logoRT.localScale = Vector3.one * introScale;
        logoCanvasGroup.alpha = 1f;
        logoCanvasGroup.blocksRaycasts = true;
        
        // 2. Hide all main screen elements immediately
        HideMainScreenImmediate();
        
        yield return new WaitForSeconds(introDelay);
        
        // 3. Move and scale logo
        float elapsed = 0f;
        Vector3 startScale = logoRT.localScale;
        Vector2 startPos = logoRT.anchoredPosition;
        
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float moveT = moveCurve.Evaluate(t);
            float scaleT = scaleCurve.Evaluate(t);
            
            logoRT.anchoredPosition = Vector2.LerpUnclamped(startPos, finalPosition, moveT);
            logoRT.localScale = Vector3.LerpUnclamped(startScale, finalScale, scaleT);
            
            yield return null;
        }
        
        logoRT.anchoredPosition = finalPosition;
        logoRT.localScale = finalScale;
        
        // 4. Hold at final position
        yield return new WaitForSeconds(holdDuration);
        
        // 5. Activate and fade in main screen elements
        yield return StartCoroutine(FadeInMainScreen());

        // Đánh dấu đã hoàn thành toàn bộ Intro lần đầu tiên
        isFirstBoot = false;
    }
    
    void HideMainScreenImmediate()
    {
        if (mainScreenElements == null) return;
        
        foreach (var obj in mainScreenElements)
        {
            if (obj == null) continue;
            
            var cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            obj.SetActive(false);
        }
    }
    
    IEnumerator FadeInMainScreen()
    {
        float elapsed = 0f;
        
        SetMainScreenAlpha(0f);
        EnableMainScreen(true);
        
        while (elapsed < mainScreenFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / mainScreenFadeInDuration);
            float fadeT = fadeInCurve.Evaluate(t);
            
            SetMainScreenAlpha(fadeT);
            yield return null;
        }
        
        SetMainScreenAlpha(1f);
        SetMainScreenInteractable(true);
        HasFinishedIntro = true;
    }
    
    void SetMainScreenAlpha(float alpha)
    {
        if (mainScreenElements == null) return;
        
        foreach (var obj in mainScreenElements)
        {
            if (obj == null) continue;
            
            var graphics = obj.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                var c = g.color;
                c.a = alpha;
                g.color = c;
            }
            
            var cg = obj.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = alpha;
        }
    }
    
    void SetMainScreenInteractable(bool interactable)
    {
        if (mainScreenElements == null) return;
        
        foreach (var obj in mainScreenElements)
        {
            if (obj == null) continue;
            
            var cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            cg.interactable = interactable;
            cg.blocksRaycasts = interactable;
        }
    }
    
    void EnableMainScreen(bool enable)
    {
        if (mainScreenElements == null) return;
        
        foreach (var obj in mainScreenElements)
        {
            if (obj == null) continue;
            if (obj.activeSelf != enable) obj.SetActive(enable);
        }
    }
}