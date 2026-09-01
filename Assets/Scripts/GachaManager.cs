using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaManager : MonoBehaviour
{
    [Header("Thẻ đặc biệt (tỉ lệ ra mặc định 5%)")]
    [Tooltip("Sprite của thẻ đặc biệt / hiếm")]
    [SerializeField] private Sprite specialCard;

    [Tooltip("Tỉ lệ xuất hiện của thẻ đặc biệt cho MỖI lượt ra bài (0.05 = 5%)")]
    [Range(0f, 1f)]
    [SerializeField] private float specialCardRate = 0.05f;

    [Tooltip("Lá bài ĐẶC BIỆT/VÀNG sẽ trượt xuống TRỄ hơn bình thường bao nhiêu giây. " +
             "Âm thanh (PlayAppearSound) KHÔNG bị ảnh hưởng, vẫn phát đúng lúc như cũ, " +
             "chỉ có hình ảnh lá bài trượt xuống chậm hơn.")]
    [SerializeField] private float specialCardRevealDelay = 0.3f;

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

    [Header("Hiệu ứng tối màn hình")]
    [Tooltip("Kéo GameObject có gắn script GachaScreenDimmer (Image phủ toàn màn hình) vào đây. " +
             "Để trống nếu chưa muốn dùng hiệu ứng này.")]
    [SerializeField] private GachaScreenDimmer screenDimmer;

    [Tooltip("Thời gian (giây) màn hình tối dần khi vừa vào scene, TRƯỚC khi thẻ đầu tiên trượt xuống")]
    [SerializeField] private float dimFadeInDuration = 0.4f;

    [Tooltip("Thời gian (giây) màn hình sáng trở lại bình thường sau khi CẢ 3 thẻ đã ra xong")]
    [SerializeField] private float dimFadeOutDuration = 0.6f;

    [Header("Hiệu ứng tia sáng xoay (God Rays)")]
    [Tooltip("Kéo GameObject có gắn script GachaGodRays vào đây. Để trống nếu chưa muốn dùng.")]
    [SerializeField] private GachaGodRays godRays;

    [Header("Âm thanh (chừa sẵn chỗ gắn SFX)")]
    [Tooltip("AudioSource để phát SFX. Kéo 1 AudioSource bất kỳ trong scene vào đây (có thể tự thêm vào chính GameObject này).")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Âm thanh phát ra khi 1 thẻ THƯỜNG xuất hiện (bắt đầu trượt xuống)")]
    [SerializeField] private AudioClip normalCardAppearSound;

    [Tooltip("Âm thanh RIÊNG phát ra khi thẻ ĐẶC BIỆT / VÀNG (tỉ lệ 5%) xuất hiện, sẽ được phát THAY cho âm thanh thường")]
    [SerializeField] private AudioClip specialCardAppearSound;

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

        // Bước vào màn Gacha -> làm tối màn hình trước, rồi mới cho thẻ đầu tiên trượt xuống
        if (screenDimmer != null)
        {
            screenDimmer.FadeIn(dimFadeInDuration);
            godRays?.FadeIn(dimFadeInDuration);
            yield return new WaitForSeconds(dimFadeInDuration);
        }

        for (int i = 0; i < 3; i++)
        {
            bool isSpecial = specialCard != null && picked[i] == specialCard;

            // Gán sprite và bật/tắt hiệu ứng chéo dành riêng cho thẻ đặc biệt
            cardImages[i].sprite = picked[i];
            SetSpecialEffect(i, isSpecial);

            // Ngay lúc thẻ bắt đầu xuất hiện: phát SFX tương ứng
            // (thẻ vàng 5% dùng âm thanh riêng thay vì âm thường)
            // LƯU Ý: âm thanh luôn phát ĐÚNG nhịp gốc này, không bị trễ theo hình ảnh bên dưới
            PlayAppearSound(isSpecial);

            // Tia sáng xoay bừng lên một nhịp mỗi khi có thẻ mới xuất hiện
            godRays?.PlayRevealPulse();

            // Thẻ đặc biệt/vàng: hình ảnh trượt xuống TRỄ hơn 1 chút (specialCardRevealDelay),
            // các thẻ thường vẫn trượt xuống ngay lập tức như cũ (delay = 0)
            float slideDelay = isSpecial ? specialCardRevealDelay : 0f;

            // Nếu là thẻ đặc biệt: cho ánh sáng vàng bùng sáng rõ hơn suốt lúc trượt xuống
            // (tính luôn cả phần delay để ánh sáng khớp với lúc thẻ THỰC SỰ bắt đầu trượt)
            if (isSpecial)
            {
                LogoLightEffect fx = (cardSpecialEffects != null && i < cardSpecialEffects.Length)
                    ? cardSpecialEffects[i] : null;
                if (fx != null)
                    fx.PlayRevealBoost(slideDelay + cardSlides[i].SlideDuration);
            }

            // Cho thẻ này trượt xuống (thẻ vàng sẽ tự đợi slideDelay giây rồi mới bắt đầu trượt)
            cardSlides[i].Play(slideDelay);

            // Đợi cho tới khi thẻ này THỰC SỰ trượt xong (gồm cả phần trễ nếu có)
            yield return new WaitForSeconds(slideDelay + cardSlides[i].SlideDuration);

            // Đợi thêm delayBetweenCards trước khi sang thẻ kế tiếp (không đợi sau thẻ cuối)
            if (i < 2)
                yield return new WaitForSeconds(delayBetweenCards);
        }

        // Cả 3 thẻ đã ra xong -> trả màn hình về sáng bình thường
        if (screenDimmer != null)
            screenDimmer.FadeOut(dimFadeOutDuration);
        godRays?.FadeOut(dimFadeOutDuration);
    }

    private void PlayAppearSound(bool isSpecial)
    {
        if (audioSource == null) return;

        AudioClip clip = isSpecial ? specialCardAppearSound : normalCardAppearSound;
        if (clip != null)
            audioSource.PlayOneShot(clip);
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