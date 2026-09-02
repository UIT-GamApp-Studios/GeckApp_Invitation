using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hiển thị ảnh hướng dẫn (tutorial) khi người chơi vào minigame LẦN ĐẦU TIÊN.
/// Từ lần chơi thứ 2 trở đi sẽ tự ẩn, không hiện lại nữa (lưu trạng thái bằng PlayerPrefs).
/// Người chơi bấm ĐÚP (double click chuột / double tap cảm ứng) vào ảnh để tắt.
///
/// CÁCH DÙNG:
/// 1. Gắn script này vào GameObject chứa ảnh tutorial (GameObject đó PHẢI có component Image).
/// 2. Kéo sprite Tutorial-media / Tutorial-dev / Tutorial-art vào Image đó.
/// 3. Đổi "Tutorial Key" trong Inspector cho từng scene, VD:
///       - MediaGameScene -> Tutorial_Media
///       - DevGameScene   -> Tutorial_Dev
///       - ArtGameScene   -> Tutorial_Art
///    (Mỗi minigame 1 key riêng để tính "đã xem" độc lập nhau.)
/// 4. Đặt GameObject này là con cuối cùng (dưới cùng danh sách Hierarchy) trong Canvas
///    để nó luôn hiện đè lên trên mọi UI khác.
/// 5. Thêm 1 Text (TMP) con bên trong, ghi sẵn nội dung "NHẤN 2 LẦN ĐỂ TẮT",
///    và tắt Raycast Target của Text đó đi để không chặn mất sự kiện click của Image cha.
/// </summary>
[RequireComponent(typeof(Image))]
public class TutorialOverlay : MonoBehaviour, IPointerClickHandler
{
    [Header("Key lưu trạng thái đã xem tutorial (mỗi minigame đặt 1 key khác nhau)")]
    [SerializeField] private string tutorialKey = "Tutorial_Media";

    private bool isShowing = false;

    private void Start()
    {
        bool hasSeenTutorial = PlayerPrefs.GetInt(tutorialKey, 0) == 1;

        if (hasSeenTutorial)
        {
            // Đã xem tutorial này rồi -> ẩn luôn, không làm gì cả
            gameObject.SetActive(false);
        }
        else
        {
            // Lần đầu vào game -> hiện tutorial và tạm dừng game lại
            isShowing = true;
            Time.timeScale = 0f;
        }
    }

    private void Update()
    {
        // Giữ game luôn ở trạng thái tạm dừng trong lúc tutorial đang hiện.
        // Đặt liên tục ở đây để không phụ thuộc thứ tự chạy Start() giữa các script khác
        // (vì 1 số GameManager cũng tự set Time.timeScale = 1 trong Start() của nó).
        if (isShowing)
        {
            Time.timeScale = 0f;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isShowing) return;

        // eventData.clickCount do EventSystem tự đếm, hoạt động cho cả double-click
        // chuột (PC) lẫn double-tap cảm ứng (mobile)
        if (eventData.clickCount >= 2)
        {
            DismissTutorial();
        }
    }

    private void DismissTutorial()
    {
        // Lưu lại là đã xem, lần sau vào minigame này sẽ không hiện nữa
        PlayerPrefs.SetInt(tutorialKey, 1);
        PlayerPrefs.Save();

        isShowing = false;
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
