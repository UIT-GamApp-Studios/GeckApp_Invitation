using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class MinigameToggleUI : MonoBehaviour
{
    [Header("Visual States")]
    [SerializeField] private GameObject onVisual;
    [SerializeField] private GameObject offVisual;

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