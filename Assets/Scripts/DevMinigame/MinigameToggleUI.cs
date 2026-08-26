using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class MinigameToggleUI : MonoBehaviour
{
    [Header("Visual States")]
    [SerializeField] private GameObject onVisual;  // Assign the Star/Green Glow graphic
    [SerializeField] private GameObject offVisual; // Assign the Blue Out graphic

    private Toggle toggle;

    public Toggle Toggle => toggle;
    public bool IsOn => toggle.isOn;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(UpdateVisuals);
        UpdateVisuals(toggle.isOn);
    }

    private void UpdateVisuals(bool isOn)
    {
        if (onVisual != null) onVisual.SetActive(isOn);
        if (offVisual != null) offVisual.SetActive(!isOn);
    }

    /// <summary>
    /// NEW: Sets the toggle's value programmatically WITHOUT firing
    /// onValueChanged (so gameplay listeners like CheckPlayerInput don't
    /// react to it), but still forces the visual to update immediately.
    ///
    /// Use this for anything game-logic-driven: hints, streak indicators,
    /// randomized initial states on a new target, etc. Do NOT set
    /// toggle.isOn directly from other scripts anymore, and do NOT call
    /// toggle.SetIsOnWithoutNotify directly either - both bypass this
    /// class's visual update and cause the visual to desync from the
    /// real toggle value (stuck showing a stale state).
    /// </summary>
    public void SetState(bool isOn)
    {
        toggle.SetIsOnWithoutNotify(isOn);
        UpdateVisuals(isOn);
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(UpdateVisuals);
    }
}