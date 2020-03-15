using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RepairHotspot : MonoBehaviour
{

    public static event Action<RepairHotspot> MouseOverRepairHotspotEnterEvent;
    public static event Action<RepairHotspot> MouseOverRepairHotspotExitEvent;
    public static event Action<RepairHotspot> MouseOverRepairHotspotClickedEvent;

    public ShipComponent shipComponent;
    //list of repair recipes to choose from
    //should populate this programmatically somehow
    //choose what type of repair can be selected possibly based on the type of ship compoenent and the location of the repair on it
    public List<RepairRecipe> repairRecipes;

    public RepairRecipe repairRecipe;

    private void Awake()
    {
        shipComponent = GetComponentInParent<ShipComponent>();
        if (shipComponent == null)
        {
            Debug.Log("error - repair hotspot has no ship component reference");
        }

        SelectRepairRecipe();
    }

    private void Start()
    {
        //foreach (RepairStage stage in currentRepairRecipe.repairStages)
        //{
        //    Debug.Log(stage.repairStageDescription);

        //    //Debug.Log(repairRequirement.repairRequirementDescription);
        //    Debug.Log(stage.repairRequirement.tool.ToString());

        //    foreach (PartRequirement partRequirement in stage.repairRequirement.partRequirements)
        //    {
        //        Debug.Log(partRequirement.part.ToString() + " Num Parts: " + partRequirement.numParts);

        //        partRequirement.numParts = Random.Range(partRequirement.minNumParts, partRequirement.maxNumParts);
        //    }
        //}
    }

    void SelectRepairRecipe()
    {
        repairRecipe = Instantiate(repairRecipes[Random.Range(0, repairRecipes.Count)]);

        foreach(RepairStage stage in repairRecipe.repairStages)
        {
            foreach(PartRequirement partRequirement in stage.repairRequirement.partRequirements)
            {
                partRequirement.parts.amount = Random.Range(partRequirement.minNumParts, partRequirement.maxNumParts);
            }
        }
    }

    void OnMouseEnter()
    {
        if(MouseOverRepairHotspotEnterEvent != null)
        {
            MouseOverRepairHotspotEnterEvent.Invoke(this);
        }
    }

    void OnMouseExit()
    {
        if(MouseOverRepairHotspotExitEvent != null)
        {
            MouseOverRepairHotspotExitEvent.Invoke(this);
        }
    }

    void OnMouseUpAsButton()
    {
        if(MouseOverRepairHotspotClickedEvent != null)
        {
            MouseOverRepairHotspotClickedEvent.Invoke(this);
        }
    }
}
