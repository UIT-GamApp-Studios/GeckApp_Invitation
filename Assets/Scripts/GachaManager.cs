using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaManager : MonoBehaviour
{
    [Header("Thẻ đặc biệt (tỉ lệ ra mặc định 5%)")]
    [SerializeField] private Sprite specialCard;
    [Range(0f, 1f)]
    [SerializeField] private float specialCardRate = 0.05f;
    [SerializeField] private float specialCardRevealDelay = 0.3f;

    [Header("Các lá bài thường")]
    [SerializeField] private Sprite[] normalCardPool;

    [Header("3 Image hiển thị lá bài")]
    [SerializeField] private Image[] cardImages;

    [Header("3 script GachaCardSlideIn tương ứng")]
    [SerializeField] private GachaCardSlideIn[] cardSlides;

    [Header("Hiệu ứng ánh sáng chéo cho thẻ đặc biệt")]
    [SerializeField] private LogoLightEffect[] cardSpecialEffects;

    [Header("Thời gian giữa các lượt ra bài")]
    [SerializeField] private float delayBetweenCards = 3f;

    [Header("Hiệu ứng tối màn hình")]
    [SerializeField] private GachaScreenDimmer screenDimmer;
    [SerializeField] private float dimFadeInDuration = 0.4f;
    [SerializeField] private float dimFadeOutDuration = 0.6f;

    [Header("Hiệu ứng tia sáng xoay (God Rays)")]
    [SerializeField] private GachaGodRays godRays;

    [Header("Âm thanh")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip normalCardAppearSound;
    [SerializeField] private AudioClip specialCardAppearSound;

    private void Start()
    {
        // Kiểm tra xem đã từng quay Gacha và lưu kết quả chưa
        if (GachaRewardManager.Instance != null && GachaRewardManager.Instance.HasSavedGachaResult())
        {
            ShowSavedResultDirectly();
        }
        else
        {
            StartCoroutine(DrawThreeCardsSequentially());
        }
    }

    // HÀM CHO LẦN ĐẦU TIÊN: Quay bài + Chạy Animation + Lưu kết quả
    private IEnumerator DrawThreeCardsSequentially()
    {
        if (cardImages.Length != 3 || cardSlides.Length != 3) yield break;

        List<Sprite> picked = PickThreeUniqueCards();
        if (picked == null) yield break;

        // Lưu tên 3 Sprite kết quả vào PlayerPrefs
        List<string> cardNamesToSave = new List<string>();
        foreach (var card in picked)
        {
            cardNamesToSave.Add(card.name);
        }
        GachaRewardManager.Instance?.SaveGachaResult(cardNamesToSave);

        // Hiệu ứng màn hình tối
        if (screenDimmer != null)
        {
            screenDimmer.FadeIn(dimFadeInDuration);
            godRays?.FadeIn(dimFadeInDuration);
            yield return new WaitForSeconds(dimFadeInDuration);
        }

        for (int i = 0; i < 3; i++)
        {
            bool isSpecial = specialCard != null && picked[i] == specialCard;

            cardImages[i].sprite = picked[i];
            SetSpecialEffect(i, isSpecial);
            PlayAppearSound(isSpecial);
            godRays?.PlayRevealPulse();

            float slideDelay = isSpecial ? specialCardRevealDelay : 0f;

            if (isSpecial)
            {
                LogoLightEffect fx = (cardSpecialEffects != null && i < cardSpecialEffects.Length) ? cardSpecialEffects[i] : null;
                if (fx != null) fx.PlayRevealBoost(slideDelay + cardSlides[i].SlideDuration);
            }

            cardSlides[i].Play(slideDelay);
            yield return new WaitForSeconds(slideDelay + cardSlides[i].SlideDuration);

            if (i < 2) yield return new WaitForSeconds(delayBetweenCards);
        }

        if (screenDimmer != null) screenDimmer.FadeOut(dimFadeOutDuration);
        godRays?.FadeOut(dimFadeOutDuration);
    }

    // HÀM CHO LẦN 2 TRỞ ĐI: Nhảy thẳng ra kết quả (Bỏ qua hiệu ứng trượt & âm thanh)
    private void ShowSavedResultDirectly()
    {
        List<string> savedNames = GachaRewardManager.Instance.GetSavedGachaResult();
        if (savedNames == null || savedNames.Count < 3) return;

        for (int i = 0; i < 3; i++)
        {
            Sprite targetSprite = FindSpriteByName(savedNames[i]);
            if (targetSprite != null)
            {
                cardImages[i].sprite = targetSprite;

                bool isSpecial = specialCard != null && targetSprite == specialCard;
                SetSpecialEffect(i, isSpecial);

                // 1. Kích hoạt GameObject
                cardImages[i].gameObject.SetActive(true);
                if (cardSlides[i] != null)
                {
                    cardSlides[i].gameObject.SetActive(true);
                }

                // 2. Bật Alpha = 1 nếu có CanvasGroup
                CanvasGroup cg = cardImages[i].GetComponent<CanvasGroup>();
                if (cg == null && cardSlides[i] != null) cg = cardSlides[i].GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }

                // 3. Đưa RectTransform về Scale chuẩn và Vị trí gốc (AnchoredPosition / LocalPosition)
                RectTransform rect = cardImages[i].rectTransform;
                if (rect != null)
                {
                    rect.localScale = Vector3.one;
                }

                // 4. Gọi Snap vị trí nếu script GachaCardSlideIn có hỗ trợ
                if (cardSlides[i] != null)
                {
                    cardSlides[i].SnapToFinalPosition();
                }
            }
        }

        // Đảm bảo không bị dính hiệu ứng tối màn hình ở lần vào lại
        if (screenDimmer != null)
        {
            screenDimmer.gameObject.SetActive(false);
        }
    }

    // Tìm Sprite dựa vào tên đã lưu
    private Sprite FindSpriteByName(string spriteName)
    {
        if (specialCard != null && specialCard.name == spriteName)
            return specialCard;

        if (normalCardPool != null)
        {
            foreach (var sprite in normalCardPool)
            {
                if (sprite != null && sprite.name == spriteName) return sprite;
            }
        }
        return null;
    }

    private void PlayAppearSound(bool isSpecial)
    {
        if (audioSource == null) return;
        AudioClip clip = isSpecial ? specialCardAppearSound : normalCardAppearSound;
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    private void SetSpecialEffect(int index, bool isSpecial)
    {
        if (cardSpecialEffects == null || index >= cardSpecialEffects.Length) return;
        LogoLightEffect fx = cardSpecialEffects[index];
        if (fx == null) return;
        fx.enabled = isSpecial;
    }

    private List<Sprite> PickThreeUniqueCards()
    {
        if (normalCardPool == null || normalCardPool.Length < 2) return null;

        List<Sprite> availableNormal = new List<Sprite>(normalCardPool);
        bool specialAvailable = specialCard != null;
        List<Sprite> result = new List<Sprite>(3);

        for (int i = 0; i < 3; i++)
        {
            bool rollSpecial = specialAvailable && Random.value < specialCardRate;

            if (rollSpecial)
            {
                result.Add(specialCard);
                specialAvailable = false;
            }
            else
            {
                if (availableNormal.Count == 0)
                {
                    if (specialAvailable)
                    {
                        result.Add(specialCard);
                        specialAvailable = false;
                        continue;
                    }
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