using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gắn script này vào nút Gacha trên MainScreen.
/// Khi bấm: kiểm tra đủ token thì trừ token và load scene Gacha.
/// </summary>
[RequireComponent(typeof(Button))]
public class GachaButtonController : MonoBehaviour
{
    [Tooltip("Số token cần để quay gacha 1 lần")]
    [SerializeField] private int costPerRoll = 3;

    [Tooltip("Tên scene Gacha, phải trùng tên file .unity và đã add vào Build Settings")]
    [SerializeField] private string gachaSceneName = "GachaScene";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnGachaButtonClicked);
    }

    private void OnEnable()
    {
        TokenManager.OnTokenChanged += UpdateInteractable;

        if (TokenManager.Instance != null)
            UpdateInteractable(TokenManager.Instance.CurrentTokens);
    }

    private void OnDisable()
    {
        TokenManager.OnTokenChanged -= UpdateInteractable;
    }

    private void UpdateInteractable(int currentTokens)
    {
        // Tự động mờ/khoá nút khi chưa đủ token, tự sáng lại khi đủ
        button.interactable = currentTokens >= costPerRoll;
    }

    private void OnGachaButtonClicked()
    {
        if (TokenManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy TokenManager trong scene.");
            return;
        }

        if (TokenManager.Instance.CurrentTokens < costPerRoll)
        {
            Debug.Log("Chưa đủ token để quay gacha.");
            return;
        }

        TokenManager.Instance.SpendTokens(costPerRoll);
        SceneManager.LoadScene(gachaSceneName);
    }
}
