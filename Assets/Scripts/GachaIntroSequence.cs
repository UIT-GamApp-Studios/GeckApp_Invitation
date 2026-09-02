using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GachaIntroSequence : MonoBehaviour
{
    [Header("Logo Hook")]
    public LogoIntroAnimation logoAnimator;

    [Header("Gacha Icon")]
    public RectTransform gachaIcon;
    public float gachaSpinRevolutions = 3f;
    public float gachaSpinDurationPerRev = 0.35f;
    public float gachaSpinEaseTime = 0.35f;
    public float gachaIdleAfterSpin = 0.25f;

    [Header("Reveal Order")]
    public RectTransform[] revealButtons;
    public float betweenButtonDelay = 0.45f;

    [Header("Per Button Reveal")]
    public float buttonRevealDuration = 0.45f;
    public float buttonOvershootScale = 1.15f;
    public float buttonDriftDistance = 18f;

    [Header("Gacha Shake (per button)")]
    public float shakeDuration = 0.4f;
    public float shakeMagnitude = 6f;
    public float shakeFrequency = 22f;

    [Header("Audio (optional)")]
    public AudioSource revealAudio;
    public AudioSource tickAudio;

    private void Awake()
    {
        HideGachaAndButtonsImmediate();
    }

    private void OnEnable()
    {
        if (logoAnimator == null) return;

        // Nếu KHÔNG PHẢI lần đầu mở game -> Bỏ qua animation và cập nhật màu xám lập tức
        if (!LogoIntroAnimation.IsFirstBoot)
        {
            SkipGachaSequenceImmediate();
        }
        else
        {
            // Lần đầu mở game -> Chờ Logo xong rồi mới chạy Animation xoay + bung nút
            StartCoroutine(WaitForLogoThenPlay());
        }
    }

    private IEnumerator WaitForLogoThenPlay()
    {
        if (gachaIcon != null)
        {
            gachaIcon.localRotation = Quaternion.identity;
            gachaIcon.localScale = Vector3.one;
        }

        float guard = 0f;
        while (logoAnimator != null && !logoAnimator.HasFinishedIntro && guard < 10f)
        {
            guard += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.15f);
        yield return StartCoroutine(PlayGachaThenReveal());
    }

    private void SkipGachaSequenceImmediate()
    {
        ActivateGachaIcon();

        if (revealButtons != null)
        {
            foreach (var btn in revealButtons)
            {
                if (btn == null) continue;
                btn.gameObject.SetActive(true);
                btn.localScale = Vector3.one;

                var cg = EnsureCanvasGroup(btn);
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        // Hoãn 1 frame để đảm bảo dữ liệu Token/Controller đã load xong trước khi cập nhật màu xám
        StartCoroutine(RefreshControllerStateDelayed());
    }

    public IEnumerator PlayGachaThenReveal()
    {
        if (gachaIcon != null)
        {
            ActivateGachaIcon();
            yield return StartCoroutine(SpinGachaIcon());
            yield return new WaitForSeconds(gachaIdleAfterSpin);
        }

        if (revealButtons != null)
        {
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

        // Sau khi hoàn thành TOÀN BỘ animation mới cập nhật màu xám/bật tắt tương tác nút
        RefreshControllerState();
    }

    private IEnumerator RefreshControllerStateDelayed()
    {
        yield return new WaitForEndOfFrame();
        RefreshControllerState();
    }

    private void RefreshControllerState()
    {
        PrivateSceneController sceneController = Object.FindFirstObjectByType<PrivateSceneController>();
        if (sceneController != null)
        {
            sceneController.UpdateGachaButtonState();
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

        while (elapsed < easeTime && easeTime > 0f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / easeTime);
            float curveT = t * t * (3f - 2f * t);
            float angle = startZ + totalRotation * 0.25f * curveT;
            gachaIcon.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

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

        float settleTime = 0.15f;
        float settleElapsed = 0f;
        float startAngle = gachaIcon.localEulerAngles.z;
        if (startAngle > 180f) startAngle -= 360f;
        float targetAngle = Mathf.Round(startAngle / 90f) * 90f;
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

    private void ActivateGachaIcon()
    {
        if (gachaIcon == null) return;

        if (!gachaIcon.gameObject.activeSelf)
        {
            gachaIcon.gameObject.SetActive(true);
        }

        var cg = EnsureCanvasGroup(gachaIcon);
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // Ép Nút Gacha luôn active/sáng màu trong suốt quá trình chạy animation
        var btn = gachaIcon.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
        }

        gachaIcon.localRotation = Quaternion.identity;
        gachaIcon.localScale = Vector3.one;
    }

    private IEnumerator RevealSingleButton(RectTransform btn)
    {
        if (btn == null) yield break;

        if (!btn.gameObject.activeSelf)
        {
            btn.gameObject.SetActive(true);
        }

        var cg = EnsureCanvasGroup(btn);
        cg.interactable = true;
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
        cg.blocksRaycasts = true;
    }

    private static float EaseOutBack(float t, float overshoot)
    {
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