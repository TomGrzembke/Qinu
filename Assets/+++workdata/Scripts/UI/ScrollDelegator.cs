using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollDelegator : MonoBehaviour, IScrollHandler
{
    [SerializeField] ScrollRect parentScrollRect;

    public void OnScroll(PointerEventData eventData)
    {
        if (parentScrollRect == null) return;

        parentScrollRect.OnScroll(eventData);
    }
}