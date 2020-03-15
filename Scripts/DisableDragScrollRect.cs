using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisableDragScrollRect : ScrollRect
{

    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }
}
