using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Random = UnityEngine.Random;

public class UsableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static event Action<Tool> OnToolIconUsed;
    public static event Action<Tool> OnToolIconRemoved;
    public static event Action<Part, int> OnPartIconUsed;
    public static event Action<Part, int> OnPartIconRemoved;

    public bool isInstance = false;
    public string clickSound;
    public string dropSound;
    public string buttonClickIncreaseSound;
    public string buttonClickDecreaseSound;

    public DropType dropType;
    public Tool tool;
    public Part part;
    public int amount;

    [HideInInspector]
    public UsableIconDropArea myDropArea;
    [HideInInspector]
    public RectTransform myDropAreaRect;
    [HideInInspector]
    public Transform myDropAreaParentTransform;

    public UsableIconDropArea previousDropArea;
    public RectTransform previousDropAreaRect;
    public Transform previousDropAreaParentTransform;

    public Transform parentReference;
    public Transform parentOriginal;

    [HideInInspector]
    public TMP_Text amountText;

    Vector2 mySizeDelta;

    Vector3 startPosition;
    Vector3 startLocalPosition;

    Image iconImage;

    bool beingDragged;

    void Start()
    {
        startLocalPosition = transform.localPosition;

        mySizeDelta = GetComponent<RectTransform>().sizeDelta;

        iconImage = GetComponent<Image>();
        //iconImage.color = Random.ColorHSV();

        beingDragged = false;

        UsableIconDropArea.OnDropOnUsableIconDropArea -= GetUsableIconDropAreaInfo;
        UsableIconDropArea.OnDropOnUsableIconDropArea += GetUsableIconDropAreaInfo;
    }

    public void GetUsableIconDropAreaInfo(UsableIconDropArea dropArea, RectTransform dropAreaRect, Transform dropParentTransform)
    {

        if (beingDragged)
        {
            myDropArea = dropArea;
            myDropAreaRect = dropAreaRect;
            myDropAreaParentTransform = dropParentTransform;
        }
    }

    public void InstantiateUsableIcon()
    {
        //create instance of icon and place in slot
        GameObject iconInstance = Instantiate(gameObject, myDropAreaParentTransform);
        iconInstance.transform.localPosition = Vector3.zero;
        iconInstance.transform.position = Vector3.zero;

        UsableIcon iconInstanceUsableIcon = iconInstance.GetComponent<UsableIcon>();
        iconInstanceUsableIcon.isInstance = true;

        Image iconInstanceImage = iconInstance.GetComponent<Image>();
        iconInstanceImage.raycastTarget = true;
    }

    
    public void OnBeginDrag(PointerEventData eventData)
    {
        SoundManager.instance.PlaySound(clickSound);

        previousDropArea = myDropArea;
        previousDropAreaRect = myDropAreaRect;
        previousDropAreaParentTransform = myDropAreaParentTransform;

        beingDragged = true;
        myDropArea = null;
        myDropAreaRect = null;

        if(dropType == DropType.TOOL)
        {
            OnToolIconRemoved.Invoke(tool);
        }
        //else if(dropType == DropType.PART)
        //{
        //    OnPartIconRemoved.Invoke(part, amount);
        //}
    }

    public void OnDrag(PointerEventData eventData)
    {
        iconImage.raycastTarget = false;
        transform.SetParent(parentReference, true);
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SoundManager.instance.PlaySound(dropSound);


        beingDragged = false;

        //checking an icon from one of the main panels
        if (isInstance == false)
        {
            if (myDropArea != null)
            {
                //check if drop area is the right type for this icon
                if (myDropArea.dropType == dropType)
                {
                    if (dropType == DropType.TOOL)
                    {
                        //put the tool in the drop area, check for duplicates when dropping in the same drop area
                        //you can drop again in a different drop area for a different repair job, figure out how to do this
                        transform.parent = myDropAreaParentTransform;
                        iconImage.raycastTarget = true;

                        //add to tools list
                        OnToolIconUsed.Invoke(tool);

                    }
                    else if (dropType == DropType.PART)
                    {
                        InstantiateUsableIcon();

                        //add to parts list
                        OnPartIconUsed.Invoke(part, amount);

                        //return original to parts panel
                        ResetIconPosition();
                    }
                }
                else
                {
                    ResetIconPosition();
                }
            }
            else
            {
                ResetIconPosition();
            }
        }
        else
        {
            //non instance icons are always parts of 1 or more amounts
            //tools will have multiple singular icons, always have an amount of 1 and never be an instance (isInstance always false)
            if(myDropArea != null)
            {
                if(myDropArea.dropType == dropType)
                {
                    transform.parent = myDropAreaParentTransform;
                    transform.localPosition = Vector3.zero;
                    transform.position = Vector3.zero;
                    iconImage.raycastTarget = true;
                }
                else
                {
                    transform.parent = previousDropAreaParentTransform;
                    transform.localPosition = Vector3.zero;
                    transform.position = Vector3.zero;
                    iconImage.raycastTarget = true;

                    myDropArea = previousDropArea;
                    myDropAreaRect = previousDropAreaRect;
                    myDropAreaParentTransform = previousDropAreaParentTransform;
                }

                //OnPartIconUsed.Invoke(part, amount);
            }
            else
            {
                OnPartIconRemoved.Invoke(part, amount);
                Destroy(gameObject);
            }
        }
    }

    public void ResetIconPosition()
    {
        iconImage.raycastTarget = true;
        transform.SetParent(parentOriginal);
        GetComponent<RectTransform>().sizeDelta = mySizeDelta;
        transform.localPosition = startLocalPosition;
    }

    public void IncreaseAmount()
    {
        amount++;
        if(amount > 1000)
        {
            amount = 1000;
        }

        amountText.text = amount.ToString();
    }

    public void DecreaseAmount()
    {
        amount--;
        if (amount < 1)
        {
            amount = 1;
        }

        amountText.text = amount.ToString();
    }

    public void PlayButtonIncreaseSound()
    {
        SoundManager.instance.PlaySound(buttonClickIncreaseSound);
    }

    public void PlayButtonDecreaseSound()
    {
        SoundManager.instance.PlaySound(buttonClickDecreaseSound);
    }

    void OnDestroy()
    {
        //Debug.Log("destoryed icon - unsubscribing");
        UsableIconDropArea.OnDropOnUsableIconDropArea -= GetUsableIconDropAreaInfo;
    }
}