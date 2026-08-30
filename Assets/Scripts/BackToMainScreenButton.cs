using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gắn script này vào nút "Quay lại" trong scene Gacha.
/// Khi bấm sẽ load lại scene MainScreen.
/// </summary>
[RequireComponent(typeof(Button))]
public class BackToMainScreenButton : MonoBehaviour
{
    [Tooltip("Tên scene MainScreen, phải trùng tên file .unity và đã add vào Build Settings")]
    [SerializeField] private string mainScreenSceneName = "MainScreen";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnBackButtonClicked()
    {
        SceneManager.LoadScene(mainScreenSceneName);
    }
}
