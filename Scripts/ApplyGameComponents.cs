using System.Collections.Generic;
using UnityEngine;

public class ApplyGameComponents : MonoBehaviour
{

    public Material highlightMaterial;

    // Start is called before the first frame update
    void Awake()
    {
        MeshRenderer meshRenderer;
        if (transform.TryGetComponent(out meshRenderer))
        {
            MainGame.instance.shipComponentObjects.Add(transform.gameObject);

            MaterialSwitch materialSwitch = transform.gameObject.AddComponent<MaterialSwitch>();
            materialSwitch.highlightMaterial = highlightMaterial;
            materialSwitch.SetupMaterials();
        }

        ApplyMaterialSwitchToChildren(transform);
    }

    void ApplyMaterialSwitchToChildren(Transform t)
    {
        MeshRenderer meshRenderer;
        if (t.TryGetComponent(out meshRenderer))
        {
            MainGame.instance.shipComponentObjects.Add(t.gameObject);

            MaterialSwitch materialSwitch = t.gameObject.AddComponent<MaterialSwitch>();
            materialSwitch.highlightMaterial = highlightMaterial;
            materialSwitch.SetupMaterials();
        }

        foreach (Transform child in t)
        {
            ApplyMaterialSwitchToChildren(child);
        }


    }
}
