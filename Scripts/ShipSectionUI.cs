using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;

public class ShipSectionUI : MonoBehaviour
{
    public Image damagedAreaIndicator;
    public GameObject indicatorsGroup;
    public TMP_Text damagedAreaNotificationText;

    public GameObject repairInfoBox;
    public TMP_Text repairTitleText;
    public TMP_Text repairDescriptionText;
    public TMP_Text repairStageText;
    public TMP_Text repairStageDescriptionText;
    public TMP_Text repairToolsRequirementText;
    public TMP_Text repairPartsRequirementText;

    public UsableIconDropArea toolsDropArea;
    public UsableIconDropArea partsDropArea;

    public List<GameObject> damagedAreaObjects;
    public List<Image> damagedAreaIndicators;

    RepairHotspot currentHotspot;

    Vector3 damagedAreaPosition;
    Vector3 damagedAreaPositionRounded;

    MainGame mainGame;
    UILineRenderer lineRenderer;
    RectTransform repairInfoRectTransform;
    Vector3 screenOffset;

    RaycastHit damageAreaHit;

    public GameObject damagedAreaSelected;

    string damageAreaNotificationSingularText = " damaged area detected";
    string damageAreaNotificationMultipleText = " damaged areas detected";

    bool showDamageNotification = true;

    // Start is called before the first frame update
    void Start()
    {
        damagedAreaIndicators = new List<Image>();

        mainGame = MainGame.instance;

        lineRenderer = GetComponent<UILineRenderer>();
        lineRenderer.m_points[0] = Vector3.zero;
        lineRenderer.m_points[1] = Vector3.zero;

        repairInfoRectTransform = repairInfoBox.GetComponent<RectTransform>();

        //ensure you haven't subscribed twice
        RepairHotspot.MouseOverRepairHotspotClickedEvent -= ShowRepairInfoBox;
        RepairHotspot.MouseOverRepairHotspotClickedEvent += ShowRepairInfoBox;

        MainGame.CheckShipSectionRepairCompleteEvent -= ClearDropAreas;
        MainGame.CheckShipSectionRepairCompleteEvent += ClearDropAreas;
    }

    // Update is called once per frame
    void Update()
    {
        screenOffset.x = -Screen.width * 0.5f;
        screenOffset.y = -Screen.height * 0.5f;

        if(mainGame.activeRepairRecipe == null)
        {
            HideRepairInfoBox();
            UpdateDamageNotificationText();

            damagedAreaSelected = null;
            ResetLineRenderer();
        }

        if (showDamageNotification)
        {
            ShowDamageNotificationText();
            showDamageNotification = false;
        }

        if (repairInfoBox.gameObject.activeSelf)
        {
            if (damagedAreaSelected != null)
            {
                lineRenderer.SetAllDirty();
            }
        }

        if (mainGame.shipSectionViewShipComponent != null)
        {
            if (damagedAreaIndicators.Count < mainGame.shipSectionViewShipComponent.damagedAreas.Count)
            {
                for (int i = 0; i < mainGame.shipSectionViewShipComponent.damagedAreas.Count - damagedAreaObjects.Count; i++)
                {
                    Image indicator = Instantiate(damagedAreaIndicator);
                    indicator.transform.SetParent(indicatorsGroup.transform);
                    indicator.rectTransform.localPosition = Vector3.zero;
                    damagedAreaIndicators.Add(indicator);
                }
            }

            for (int i = 0; i < damagedAreaIndicators.Count; i++)
            {
                if (i < mainGame.shipSectionViewShipComponent.damagedAreas.Count)
                {
                    RepairHotspot hotspot = mainGame.shipSectionViewShipComponent.damagedAreas[i].GetComponent<RepairHotspot>();
                    if (hotspot.repairRecipe.repairComplete)
                    {
                        damagedAreaIndicators[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        Vector3 direction = (mainGame.currentCamera.transform.position - mainGame.shipSectionViewShipComponent.damagedAreas[i].transform.position).normalized;
                        if (Physics.Raycast(mainGame.shipSectionViewShipComponent.damagedAreas[i].transform.position + (direction * 0.2f), direction, out damageAreaHit))
                        {
                            damagedAreaIndicators[i].gameObject.SetActive(false);

                            if(damagedAreaSelected != null)
                            {
                                if (mainGame.shipSectionViewShipComponent.damagedAreas[i].GetInstanceID() == damagedAreaSelected.GetInstanceID())
                                {
                                    lineRenderer.m_points[0] = Vector3.zero;
                                    lineRenderer.m_points[1] = Vector3.zero;
                                }
                            }
                        }
                        else
                        {
                            damagedAreaPosition = mainGame.currentCamera.WorldToScreenPoint(mainGame.shipSectionViewShipComponent.damagedAreas[i].transform.position);

                            damagedAreaPositionRounded.x = Mathf.Round(damagedAreaPosition.x);
                            damagedAreaPositionRounded.y = Mathf.Round(damagedAreaPosition.y);
                            damagedAreaPositionRounded.z = Mathf.Round(damagedAreaPosition.z);

                            damagedAreaIndicators[i].rectTransform.position = damagedAreaPositionRounded;

                            damagedAreaIndicators[i].gameObject.SetActive(true);

                            if (damagedAreaSelected != null)
                            {

                                if (mainGame.shipSectionViewShipComponent.damagedAreas[i].GetInstanceID() == damagedAreaSelected.GetInstanceID())
                                {
                                    lineRenderer.m_points[0] = repairInfoRectTransform.transform.localPosition;
                                    lineRenderer.m_points[0].y += (repairInfoRectTransform.rect.height * 0.5f);
                                    lineRenderer.m_points[1] = damagedAreaIndicators[i].transform.localPosition;
                                }
                            }

                        }
                    }
                }
                else
                {
                    damagedAreaIndicators[i].gameObject.SetActive(false);
                }
            }
        }
        
    }

    void ShowRepairInfoBox(RepairHotspot hotspot)
    {
        ShipComponent shipComp = hotspot.shipComponent;

        currentHotspot = hotspot;
        UpdateRepairInfo();

        repairInfoBox.SetActive(true);

        damagedAreaSelected = hotspot.gameObject;

        showDamageNotification = true;
    }

    public void HideRepairInfoBox()
    {
        repairInfoBox.SetActive(false);
        ResetLineRenderer();
    }

    public void UpdateRepairInfo()
    {
        RepairRecipe recipe = currentHotspot.repairRecipe;
        repairTitleText.text = recipe.repairTitle;
        repairDescriptionText.text = recipe.repairDescription;
        repairStageText.text = "Stage " + (recipe.currentRepairStage + 1).ToString();
        repairStageDescriptionText.text = recipe.repairStages[recipe.currentRepairStage].repairStageDescription;
        repairToolsRequirementText.text = recipe.repairStages[recipe.currentRepairStage].repairRequirement.toolName;
        repairPartsRequirementText.text = "";

        foreach (PartRequirement requirement in recipe.repairStages[recipe.currentRepairStage].repairRequirement.partRequirements)
        {
            string pluralQualifier = "";
            if (requirement.parts.amount > 1)
            {
                pluralQualifier = "s";
            }

            repairPartsRequirementText.text += requirement.parts.amount + " " + requirement.partName + "" + pluralQualifier + "\n";
        }
    }

    public void HideDamageIndicators()
    {
        for (int i = 0; i < damagedAreaIndicators.Count; i++)
        {
            damagedAreaIndicators[i].gameObject.SetActive(false);
        }
    }

    public void ShowDamageNotificationText()
    {
        UpdateDamageNotificationText();
        damagedAreaNotificationText.gameObject.SetActive(true);
    }

    public void HideDamageNotificationText()
    {
        damagedAreaNotificationText.gameObject.SetActive(false);
    }

    public void UpdateDamageNotificationText()
    {
        if (mainGame.shipSectionViewShipComponent.damagedAreas.Count > 0)
        {
            if (mainGame.shipSectionViewShipComponent.damagedAreas.Count == 1)
            {
                damagedAreaNotificationText.text = mainGame.shipSectionViewShipComponent.damagedAreas.Count + damageAreaNotificationSingularText;
            }
            else
            {
                damagedAreaNotificationText.text = mainGame.shipSectionViewShipComponent.damagedAreas.Count + damageAreaNotificationMultipleText;
            }
        }
        else
        {
            damagedAreaNotificationText.text = "All damaged areas repaired.";
        }
    }

    void ResetLineRenderer()
    {
        lineRenderer.m_points[0] = Vector3.zero;
        lineRenderer.m_points[1] = Vector3.zero;
        lineRenderer.SetAllDirty();
    }

    void ClearDropAreas()
    {
        toolsDropArea.ClearDropArea();
        partsDropArea.ClearDropArea();
    }

    void OnDestroy()
    {
        RepairHotspot.MouseOverRepairHotspotClickedEvent -= ShowRepairInfoBox;
        MainGame.CheckShipSectionRepairCompleteEvent -= ClearDropAreas;
    }
}
