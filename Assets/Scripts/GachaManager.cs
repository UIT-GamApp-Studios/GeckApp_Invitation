using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn script này vào 1 GameObject rỗng trong scene Gacha (scene chỉ có Background).
/// Script sẽ random 3 lá bài không trùng nhau và cho từng lá xuất hiện LẦN LƯỢT
/// (trượt từ trên xuống), thẻ này trượt xong mới đợi rồi tới thẻ tiếp theo.
/// Có 1 thẻ đặc biệt (specialCard) với tỉ lệ xuất hiện riêng (mặc định 5%),
/// khi ra thẻ này sẽ có hiệu ứng ánh sáng chạy chéo từ góc dưới trái lên góc trên phải.
/// </summary>
public class GachaManager : MonoBehaviour
{
    [Header("Thẻ đặc biệt (tỉ lệ ra mặc định 5%)")]
    [Tooltip("Sprite của thẻ đặc biệt / hiếm")]
    [SerializeField] private Sprite specialCard;

    [Tooltip("Tỉ lệ xuất hiện của thẻ đặc biệt cho MỖI lượt ra bài (0.05 = 5%)")]
    [Range(0f, 1f)]
    [SerializeField] private float specialCardRate = 0.05f;

    [Header("Các lá bài thường (KHÔNG bao gồm thẻ đặc biệt ở trên)")]
    [Tooltip("Kéo các sprite thẻ thường vào đây, tỉ lệ ra vẫn ngẫu nhiên đều như cũ")]
    [SerializeField] private Sprite[] normalCardPool;

    [Header("3 Image hiển thị lá bài, đặt đúng vị trí đích trong scene")]
    [SerializeField] private Image[] cardImages;

    [Header("3 script GachaCardSlideIn tương ứng, CÙNG THỨ TỰ với cardImages")]
    [SerializeField] private GachaCardSlideIn[] cardSlides;

    [Header("Hiệu ứng ánh sáng chéo cho thẻ đặc biệt")]
    [Tooltip("3 component LogoLightEffect gắn kèm trên từng cardImages (CÙNG THỨ TỰ), " +
             "để sẵn sweepDirection = 1 (chéo) trong Inspector. Sẽ chỉ được BẬT ở đúng ô ra thẻ đặc biệt.")]
    [SerializeField] private LogoLightEffect[] cardSpecialEffects;

    [Header("Thời gian giữa các lượt ra bài")]
    [Tooltip("Sau khi 1 thẻ trượt xong, đợi bao lâu (giây) rồi mới tới thẻ tiếp theo")]
    [SerializeField] private float delayBetweenCards = 3f;

    private void Start()
    {
        StartCoroutine(DrawThreeCardsSequentially());
    }

    private IEnumerator DrawThreeCardsSequentially()
    {
        if (cardImages.Length != 3 || cardSlides.Length != 3)
        {
            Debug.LogError("cardImages và cardSlides phải có đúng 3 phần tử.");
            yield break;
        }

        List<Sprite> picked = PickThreeUniqueCards();
        if (picked == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            bool isSpecial = specialCard != null && picked[i] == specialCard;

            // Gán sprite và bật/tắt hiệu ứng chéo dành riêng cho thẻ đặc biệt
            cardImages[i].sprite = picked[i];
            SetSpecialEffect(i, isSpecial);

            // Cho thẻ này trượt xuống
            cardSlides[i].Play();

            // Đợi cho tới khi thẻ này trượt xong
            yield return new WaitForSeconds(cardSlides[i].SlideDuration);

            // Đợi thêm delayBetweenCards trước khi sang thẻ kế tiếp (không đợi sau thẻ cuối)
            if (i < 2)
                yield return new WaitForSeconds(delayBetweenCards);
        }
    }

    private void SetSpecialEffect(int index, bool isSpecial)
    {
        if (cardSpecialEffects == null || index >= cardSpecialEffects.Length) return;
        LogoLightEffect fx = cardSpecialEffects[index];
        if (fx == null) return;

        fx.enabled = isSpecial;
    }

    /// <summary>
    /// Chọn 3 lá bài không trùng nhau. Mỗi lượt trong 3 lượt có specialCardRate cơ hội
    /// ra thẻ đặc biệt (nếu thẻ đó chưa được rút ở lượt trước); còn lại random đều
    /// trong normalCardPool giống hệt logic cũ.
    /// </summary>
    private List<Sprite> PickThreeUniqueCards()
    {
        if (normalCardPool == null || normalCardPool.Length < 2)
        {
            Debug.LogError("Cần ít nhất 2 sprite trong normalCardPool (cộng thêm thẻ đặc biệt) để quay gacha!");
            return null;
        }

        List<Sprite> availableNormal = new List<Sprite>(normalCardPool);
        bool specialAvailable = specialCard != null;
        List<Sprite> result = new List<Sprite>(3);

        for (int i = 0; i < 3; i++)
        {
            bool rollSpecial = specialAvailable && Random.value < specialCardRate;

            if (rollSpecial)
            {
                result.Add(specialCard);
                specialAvailable = false; // thẻ đặc biệt chỉ ra tối đa 1 lần / lượt quay
            }
            else
            {
                if (availableNormal.Count == 0)
                {
                    // Hết thẻ thường (trường hợp hiếm khi pool quá nhỏ) -> đành lấy thẻ đặc biệt nếu còn
                    if (specialAvailable)
                    {
                        result.Add(specialCard);
                        specialAvailable = false;
                        continue;
                    }
                    Debug.LogError("Không đủ thẻ không trùng nhau để quay gacha!");
                    return null;
                }

                int idx = Random.Range(0, availableNormal.Count);
                result.Add(availableNormal[idx]);
                availableNormal.RemoveAt(idx);
            }
        }

        return result;
    }
}
