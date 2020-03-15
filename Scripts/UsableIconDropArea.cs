using System;
using UnityEngine.EventSystems;
using UnityEngine;

[Serializable]
public enum DropType
{
    NONE,
    TOOL,
    PART
}

public class UsableIconDropArea : MonoBehaviour, IDropHandler
{

    public static event Action<UsableIconDropArea, RectTransform, Transform> OnDropOnUsableIconDropArea;

    public DropType dropType;
    public Transform dropParentTransform;

    RectTransform dropAreaRect;

    void Start()
    {
        dropAreaRect = transform as RectTransform;
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(dropAreaRect, Input.mousePosition))
        {
             OnDropOnUsableIconDropArea.Invoke(this, dropAreaRect, dropParentTransform);
        }
    }

    public void ClearDropArea()
    {
        int childCount = dropParentTransform.childCount;
        for(int i = childCount-1; i >= 0; i--)
        {
            Transform t = dropParentTransform.GetChild(i);
            UsableIcon icon = t.GetComponent<UsableIcon>();
            if (icon.isInstance == false)
            {
                icon.ResetIconPosition();
            }
            else
            {
                Destroy(t.gameObject);
            }
        }
  
    }

}