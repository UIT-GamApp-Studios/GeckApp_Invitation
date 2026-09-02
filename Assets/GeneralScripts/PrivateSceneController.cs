using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PrivateSceneController : MonoBehaviour
{
    [SerializeField] private string gameplayScene = "Gameplay";
    [SerializeField] private string gachaSceneName = "Gacha";

    [Header("Gacha UI Settings")]
    [SerializeField] private Button gachaButton;

    private void Start()
    {
        // Nếu là lần đầu vào game (đang chờ animation), giữ nút hiển thị sáng bình thường.
        // Tránh việc cập nhật xám ngay từ frame đầu tiên.
        if (LogoIntroAnimation.IsFirstBoot)
        {
            SetGachaButtonInteractable(true);
        }
        else
        {
            // Nếu từ scene khác quay lại -> Cập nhật trạng thái token/xám ngay lập tức
            UpdateGachaButtonState();
        }
    }

    // Cập nhật trạng thái nút dựa trên số Token hiện tại
    public void UpdateGachaButtonState()
    {
        if (gachaButton == null) return;

        bool isUnlocked = GachaRewardManager.Instance != null && GachaRewardManager.Instance.IsGachaUnlocked();
        gachaButton.interactable = isUnlocked;
    }

    // Cho phép ép bật/tắt interactable của Button từ bên ngoài
    public void SetGachaButtonInteractable(bool state)
    {
        if (gachaButton != null)
        {
            gachaButton.interactable = state;
        }
    }

    public void OpenGachaScene()
    {
        if (GachaRewardManager.Instance != null && GachaRewardManager.Instance.IsGachaUnlocked())
        {
            SceneTransition.Instance.PlayTransition(() => {
                Time.timeScale = 1f;
                SceneManager.LoadScene(gachaSceneName);
            });
        }
    }

    public void ChangeScene(string sceneName)
    {   
        SceneTransition.Instance.PlayTransition(() => { Time.timeScale = 1f; SceneManager.LoadScene(sceneName); });
    }

    public void PlayGame()
    {
        SceneTransition.Instance.PlayTransition(() => { SceneManager.LoadScene(gameplayScene); });
    }

    public void QuitGame()
    {
        Debug.Log("Quit game");
        SceneTransition.Instance.PlayTransition(() => { Application.Quit(); });
    } 
}