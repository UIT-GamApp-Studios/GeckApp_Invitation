using UnityEngine;
using TMPro; 

[RequireComponent(typeof(TextMeshProUGUI))]
public class TokenCounterUI : MonoBehaviour
{
    [Tooltip("Định dạng hiển thị, {0} sẽ được thay bằng số token")]
    [SerializeField] private string format = "x{0}";

    private TextMeshProUGUI label;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        TokenManager.OnTokenChanged += UpdateDisplay;

        if (TokenManager.Instance != null)
        {
            UpdateDisplay(TokenManager.Instance.CurrentTokens);
        }
    }

    private void OnDisable()
    {
        TokenManager.OnTokenChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int amount)
    {
        label.text = string.Format(format, amount);
    }
}

/*
 NẾU PROJECT DÙNG UI.Text (Legacy) THAY VÌ TextMeshPro:
 - Xóa dòng "using TMPro;"
 - Đổi tất cả "TextMeshProUGUI" thành "UnityEngine.UI.Text"
*/