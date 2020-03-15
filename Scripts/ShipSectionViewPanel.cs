using UnityEngine;
using UnityEngine.EventSystems;

public class ShipSectionViewPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        MainGame.instance.DisableShipSectionViewRotation();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.dragging == false)
        {
            MainGame.instance.EnableShipSectionViewRotation();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MainGame.instance.DisableShipSectionViewRotation();

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MainGame.instance.EnableShipSectionViewRotation();
    }


}
