using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GachaIntroSequence : MonoBehaviour
{
    [Header("Logo Hook")]
    [Tooltip("Reference to the logo intro animator. Its animation kicks off this sequence.")]
    public LogoIntroAnimation logoAnimator;

    [Header("Gacha Icon")]
    [Tooltip("The gacha icon that rotates in place after the logo settles.")]
    public RectTransform gachaIcon;
    [Tooltip("Total rotations the gacha icon makes before stopping.")]
    public float gachaSpinRevolutions = 3f;
    [Tooltip("Seconds for a single full rotation. Lower = faster spin.")]
    public float gachaSpinDurationPerRev = 0.35f;
    [Tooltip("How long to ease in/out the spin (extra time on top of the revolutions).")]
    public float gachaSpinEaseTime = 0.35f;
    [Tooltip("Seconds the gacha icon keeps spinning idly after the reveal before buttons appear.")]
    public float gachaIdleAfterSpin = 0.25f;

    [Header("Reveal Order")]
    [Tooltip("Buttons shown one-by-one in this order.")]
    public RectTransform[] revealButtons;
    [Tooltip("Seconds between each button reveal (felt delay so it feels like gacha).")]
    public float betweenButtonDelay = 0.45f;

    [Header("Per Button Reveal")]
    [Tooltip("How long each button takes to pop in / fade in.")]
    public float buttonRevealDuration = 0.45f;
    [Tooltip("Max overshoot scale during pop (1 = no overshoot, 1.15 = small punch).")]
    public float buttonOvershootScale = 1.15f;
    [Tooltip("Vertical drift the button does while fading in (down -> final).")]
    public float buttonDriftDistance = 18f;

    [Header("Gacha Shake (per button)")]
    [Tooltip("How long the gacha icon shakes when each button lands.")]
    public float shakeDuration = 0.4f;
    [Tooltip("Max rotation jitter in degrees per axis (per button landing).")]
    public float shakeMagnitude = 6f;
    [Tooltip("Shakes per second during the shake window.")]
    public float shakeFrequency = 22f;

    [Header("Audio (optional)")]
    [Tooltip("Played when the first button lands.")]
    public AudioSource revealAudio;
    [Tooltip("Played when each additional button lands.")]
    public AudioSource tickAudio;

    private void Awake()
    {
        HideGachaAndButtonsImmediate();
    }

    private void OnEnable()
    {
        if (logoAnimator == null) return;

        // Mirror LogoIntroAnimation.hasAnimated via a small pointer to avoid coupling.
        // We hook into the logo animator's start by polling until it has finished.
        StartCoroutine(WaitForLogoThenPlay());
    }

    private IEnumerator WaitForLogoThenPlay()
    {
        if (gachaIcon != null)
        {
            gachaIcon.localRotation = Quaternion.identity;
            gachaIcon.localScale = Vector3.one;
        }

        HideGachaAndButtonsImmediate();
        yield return null;
        yield return StartCoroutine(WaitForLogoSettled());
    }

    private IEnumerator WaitForLogoSettled()
    {
        float guard = 0f;
        while (logoAnimator != null && !logoAnimator.HasFinishedIntro && guard < 10f)
        {
            guard += Time.deltaTime;
            yield return null;
        }

        // Add a small settle pause before starting the gacha spin.
        yield return new WaitForSeconds(0.15f);

        yield return StartCoroutine(PlayGachaThenReveal());
    }

    public IEnumerator PlayGachaThenReveal()
    {
        if (gachaIcon != null)
        {
            yield return StartCoroutine(SpinGachaIcon());
            yield return new WaitForSeconds(gachaIdleAfterSpin);
        }

        if (revealButtons == null || revealButtons.Length == 0) yield break;

        for (int i = 0; i < revealButtons.Length; i++)
        {
            yield return StartCoroutine(RevealSingleButton(revealButtons[i]));
            yield return StartCoroutine(ShakeGacha());

            if (i == 0 && revealAudio != null) revealAudio.Play();
            else if (i > 0 && tickAudio != null) tickAudio.Play();

            if (i < revealButtons.Length - 1)
                yield return new WaitForSeconds(betweenButtonDelay);
        }
    }

    private IEnumerator SpinGachaIcon()
    {
        if (gachaIcon == null) yield break;

        float totalSpinTime = gachaSpinRevolutions * gachaSpinDurationPerRev + gachaSpinEaseTime;
        float easeTime = Mathf.Min(gachaSpinEaseTime, totalSpinTime * 0.4f);
        float constantTime = totalSpinTime - easeTime * 2f;
        if (constantTime < 0f) constantTime = 0f;

        float totalRotation = 360f * gachaSpinRevolutions;
        float elapsed = 0f;
        float startZ = gachaIcon.localEulerAngles.z;
        if (startZ > 180f) startZ -= 360f;

        // Ease-in
        while (elapsed < easeTime && easeTime > 0f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / easeTime);
            float curveT = t * t * (3f - 2f * t); // smoothstep
            float angle = startZ + totalRotation * 0.25f * curveT;
            gachaIcon.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // Constant spin
        float constantAngle = startZ + totalRotation * 0.25f;
        elapsed = 0f;
        while (elapsed < constantTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = constantTime > 0f ? Mathf.Clamp01(elapsed / constantTime) : 1f;
            float angle = Mathf.Lerp(constantAngle, startZ + totalRotation * 0.75f, t);
            gachaIcon.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        float easeStartAngle = startZ + totalRotation * 0.75f;
        elapsed = 0f;
        while (elapsed < easeTime && easeTime > 0f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / easeTime);
            float curveT = t * t * (3f - 2f * t);
            float angle = Mathf.Lerp(easeStartAngle, startZ + totalRotation, curveT);
            gachaIcon.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        gachaIcon.localRotation = Quaternion.Euler(0f, 0f, startZ + totalRotation);
    }

    private IEnumerator ShakeGacha()
    {
        if (gachaIcon == null) yield break;

        float elapsed = 0f;
        float baseAngle = gachaIcon.localEulerAngles.z;
        if (baseAngle > 180f) baseAngle -= 360f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / shakeDuration);
            float falloff = 1f - t;

            float wave = Mathf.Sin(elapsed * shakeFrequency * Mathf.PI * 2f);
            float jitter = wave * shakeMagnitude * falloff;

            gachaIcon.localRotation = Quaternion.Euler(0f, 0f, baseAngle + jitter);
            yield return null;
        }

        // Settle back smoothly to a clean angle.
        float settleTime = 0.15f;
        float settleElapsed = 0f;
        float startAngle = gachaIcon.localEulerAngles.z;
        if (startAngle > 180f) startAngle -= 360f;
        float targetAngle = Mathf.Round(startAngle / 90f) * 90f; // snap to nearest quarter turn
        while (settleElapsed < settleTime)
        {
            settleElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(settleElapsed / settleTime);
            float angle = Mathf.Lerp(startAngle, targetAngle, t);
            gachaIcon.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        gachaIcon.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    private void HideGachaAndButtonsImmediate()
    {
        if (gachaIcon != null)
        {
            var cg = EnsureCanvasGroup(gachaIcon);
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            gachaIcon.gameObject.SetActive(false);
        }

        if (revealButtons == null) return;

        foreach (var btn in revealButtons)
        {
            if (btn == null) continue;
            var cg = EnsureCanvasGroup(btn);
            cg.alpha = 0f;
            btn.localScale = Vector3.one * 0.6f;
            btn.gameObject.SetActive(false);
        }
    }

    private IEnumerator RevealSingleButton(RectTransform btn)
    {
        if (btn == null) yield break;

        if (!btn.gameObject.activeSelf)
        {
            btn.gameObject.SetActive(true);
        }

        var cg = EnsureCanvasGroup(btn);
        cg.interactable = false;
        cg.blocksRaycasts = false;

        Vector2 finalAnchored = btn.anchoredPosition;
        Vector2 startAnchored = finalAnchored + Vector2.down * buttonDriftDistance;

        btn.anchoredPosition = startAnchored;
        btn.localScale = Vector3.one * 0.6f;
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < buttonRevealDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / buttonRevealDuration);
            float tOvershoot = EaseOutBack(t, buttonOvershootScale);
            float tFade = t * t * (3f - 2f * t);

            btn.anchoredPosition = Vector2.LerpUnclamped(startAnchored, finalAnchored, t);
            btn.localScale = Vector3.one * tOvershoot;
            cg.alpha = tFade;

            yield return null;
        }

        btn.anchoredPosition = finalAnchored;
        btn.localScale = Vector3.one;
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private static float EaseOutBack(float t, float overshoot)
    {
        // Standard ease-out-back where overshoot ~ 1.7 maps to the canonical curve.
        // We rescale it so overshoot == 1 means no overshoot.
        float s = Mathf.Lerp(1f, 1.70158f, Mathf.InverseLerp(1f, 1.15f, overshoot));
        float u = t - 1f;
        return 1f + (s + 1f) * u * u * u + s * u * u;
    }

    private static CanvasGroup EnsureCanvasGroup(RectTransform rt)
    {
        var cg = rt.GetComponent<CanvasGroup>();
        if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }
}
