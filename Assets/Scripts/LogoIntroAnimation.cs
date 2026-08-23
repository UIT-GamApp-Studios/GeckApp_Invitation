using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LogoIntroAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float introDelay = 0.2f;
    public float moveDuration = 1.0f;
    public float holdDuration = 0.3f;
    public float mainScreenFadeInDuration = 0.6f;
    
    [Header("Center Position")]
    [Tooltip("Center of screen for intro logo")]
    public Vector2 centerPosition = Vector2.zero;
    [Tooltip("Initial scale of logo in intro (large, centered)")]
    public float introScale = 1.4f;
    
    [Header("Easing")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Main Screen Elements")]
    [Tooltip("All elements that should fade in after intro")]
    public GameObject[] mainScreenElements;
    
    private RectTransform logoRT;
    private CanvasGroup logoCanvasGroup;
    private Vector2 finalPosition;
    private Vector3 finalScale;
    private bool hasAnimated = false;

    public bool HasFinishedIntro { get; private set; } = false;
    
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
        if (!hasAnimated)
        {
            StartCoroutine(PlayIntro());
        }
    }
    
    IEnumerator PlayIntro()
    {
        hasAnimated = true;
        
        // 1. Setup initial state: logo in center, scaled up, fully visible
        logoRT.anchoredPosition = centerPosition;
        logoRT.localScale = Vector3.one * introScale;
        logoCanvasGroup.alpha = 1f;
        logoCanvasGroup.blocksRaycasts = true;
        
        // 2. Hide all main screen elements immediately - set inactive and zero alpha
        HideMainScreenImmediate();
        
        // Small delay to ensure render setup
        yield return new WaitForSeconds(introDelay);
        
        // 3. Move and scale logo from center to final position
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
        
        // Ensure exact final values
        logoRT.anchoredPosition = finalPosition;
        logoRT.localScale = finalScale;
        
        // 4. Hold at final position briefly
        yield return new WaitForSeconds(holdDuration);
        
        // 5. Activate and fade in main screen elements
        yield return StartCoroutine(FadeInMainScreen());
    }
    
    void HideMainScreenImmediate()
    {
        if (mainScreenElements == null) return;
        
        foreach (var obj in mainScreenElements)
        {
            if (obj == null) continue;
            
            // Ensure CanvasGroup exists and is fully transparent
            var cg = obj.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = obj.AddComponent<CanvasGroup>();
            }
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            
            // Disable the GameObject so nothing renders during intro
            obj.SetActive(false);
        }
    }
    
    IEnumerator FadeInMainScreen()
    {
        float elapsed = 0f;
        
        // Pre-setup: enable but transparent
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
            
            // Also set CanvasGroup if present
            var cg = obj.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = alpha;
            }
        }
    }
    
    void SetMainScreenInteractable(bool interactable)
    {
        if (mainScreenElements == null) return;
        
        foreach (var obj in mainScreenElements)
        {
            if (obj == null) continue;
            
            var cg = obj.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.interactable = interactable;
                cg.blocksRaycasts = interactable;
            }
            else
            {
                obj.AddComponent<CanvasGroup>().interactable = interactable;
                obj.GetComponent<CanvasGroup>().blocksRaycasts = interactable;
            }
        }
    }
    
    void EnableMainScreen(bool enable)
    {
        if (mainScreenElements == null) return;
        
        foreach (var obj in mainScreenElements)
        {
            if (obj == null) continue;
            
            if (obj.activeSelf != enable)
            {
                obj.SetActive(enable);
            }
        }
    }
}