using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(CanvasGroup))]
public class UIDraggableColor : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ColorId colorId;
    public ColorId ColorId => colorId;

    private Canvas canvas;
    private GameObject dragProxy;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragProxy = Instantiate(gameObject, canvas.transform);
        CanvasGroup proxyGroup = dragProxy.GetComponent<CanvasGroup>();
        proxyGroup.blocksRaycasts = false; 
        proxyGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragProxy != null)
        {
            dragProxy.GetComponent<RectTransform>().position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragProxy != null) Destroy(dragProxy);
    }
}