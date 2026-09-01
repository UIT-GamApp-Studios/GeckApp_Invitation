using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public enum SlotState { Empty, Single, Resolved }

[RequireComponent(typeof(Image))]
public class MixSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image slotImage;
    [SerializeField] private Sprite emptySprite; 
    
    public bool acceptsDrops = false;
    public SlotState CurrentState { get; private set; } = SlotState.Empty;
    public ColorId CurrentColor { get; private set; } = ColorId.None;

    // Fired on EVERY successful drop
    public event Action<MixSlot> OnSlotChanged; 

    public void ResetSlot()
    {
        CurrentState = SlotState.Empty;
        CurrentColor = ColorId.None;
        acceptsDrops = false;
        
        slotImage.sprite = emptySprite;
    }

    public void SetActive(bool active)
    {
        acceptsDrops = active;
    }

    public void UpdateVisual(Sprite newSprite)
    {
        if (newSprite != null)
        {
            slotImage.sprite = newSprite;
            slotImage.color = Color.white;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!acceptsDrops) return;

        UIDraggableColor draggedColor = eventData.pointerDrag?.GetComponent<UIDraggableColor>();
        if (draggedColor == null) return;

        if (CurrentState == SlotState.Empty)
        {
            CurrentState = SlotState.Single;
            CurrentColor = draggedColor.ColorId;
            OnSlotChanged?.Invoke(this);
        }
        else if (CurrentState == SlotState.Single)
        {
            if (ColorRecipeDatabase.TryGetMixResult(CurrentColor, draggedColor.ColorId, out ColorId finalColor))
            {
                CurrentState = SlotState.Resolved;
                CurrentColor = finalColor;
                
                OnSlotChanged?.Invoke(this);
            }
        }
    }
}