using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum ShipComponentType
{
    NONE = 0,
    PRIMARY = 1,
    SECONDARY = 2,
    SYSTEM = 4,
    INSTRUMENTATION = 8
}

public enum DamageType
{
    NONE,
    BROKEN,
    DESTROYED,
}

public enum DamageSeverity
{
    NONE,
    LIGHT,
    MODERATE,
    HEAVY
}

public class ShipComponent : MonoBehaviour
{

    public static event Action<GameObject> SelectedDamagedShipComponentEvent;

    [HideInInspector]
    public int ID = -1;

    [HideInInspector]
    public Transform originalTransform;

    public GameObject instanceObject;
    public bool isMirrored = false;
    [HideInInspector]
    public bool isInstanced = false;
    [HideInInspector]
    public bool isSelectable = true;

    public ShipComponentType sectionType;
    //[HideInInspector]
    public bool isDamaged = false;

    public List<GameObject> damagedAreas;

    MaterialSwitch materialSwitch;

    // Start is called before the first frame update
    void Start()
    {
        materialSwitch = GetComponent<MaterialSwitch>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetDamaged()
    {
        isDamaged = true;
        materialSwitch.canHighlight = true;
    }

    private void OnMouseUpAsButton()
    {
        if(isSelectable && isDamaged)
        {
            Debug.Log("selected damaged ship component");
            SelectedDamagedShipComponentEvent.Invoke(gameObject);
        }
    }
}
