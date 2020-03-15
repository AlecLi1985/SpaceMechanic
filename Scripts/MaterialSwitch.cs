using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitch : MonoBehaviour
{

    public Material highlightMaterial { get; set; }
    public bool canHighlight = false;

    MeshRenderer meshRenderer;
    Texture startBaseMap;
    Texture startMetallicMap;
    Texture startNormalMap;

    public List<Material> m;

    public void SetupMaterials()
    {
        meshRenderer = transform.GetComponent<MeshRenderer>();

        m = new List<Material>();
        m.Add(meshRenderer.material);
        m.Add(highlightMaterial);

        startBaseMap = m[0].GetTexture("_BaseMap");
        startMetallicMap = m[0].GetTexture("_MetallicGlossMap");
        startNormalMap = m[0].GetTexture("_BumpMap");

    }

    public void SetHighlightColor(Color color)
    {
        MaterialPropertyBlock colorBlock = new MaterialPropertyBlock();
        colorBlock.SetVector("_fresnelColor", color);
        meshRenderer.SetPropertyBlock(colorBlock);
    }

    private void OnMouseOver()
    {
        if(canHighlight)
        {
            m[1].SetTexture("_baseMap", startBaseMap);
            m[1].SetTexture("_metallicMap", startMetallicMap);
            m[1].SetTexture("_normalMap", startNormalMap);
            meshRenderer.material = m[1];
        }
    }

    private void OnMouseExit()
    {
        if(canHighlight)
        {
            meshRenderer.material = m[0];
        }
    }
}
