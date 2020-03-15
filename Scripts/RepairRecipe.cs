using System;
using System.Collections.Generic;
using UnityEngine;

public enum Tool
{
    NONE,
    PLASMA_TORCH,
    SEALANT_GUN
}

public enum Part
{
    NONE,
    HULL_PANEL,
    LIQUIDALLOY_SEALANT,
    FUEL_CONDUIT,
    ELECTRICAL_CONDUIT,
    COAXIAL_FIBRE,
    DATA_CHIP,

}

[Serializable]
public struct PartsAmount
{
    public Part part;
    public int amount;
}

[Serializable]
public class PartRequirement
{
    [SerializeField]
    public PartsAmount parts;
    [SerializeField]
    public string partName;

    [SerializeField]
    public int minNumParts;

    [SerializeField]
    public int maxNumParts;
}

[Serializable]
public class RepairRequirement
{
    public bool requirementMet { get; set; }

    //[SerializeField]
    //public string repairRequirementDescription;

    [SerializeField]
    public Tool tool;
    [SerializeField]
    public string toolName;

    [SerializeField]
    public PartRequirement[] partRequirements;
}

[Serializable]
public class RepairStage
{
    public bool stageComplete { get; set; }

    [SerializeField]
    public string repairStageDescription;

    [SerializeField]
    public RepairRequirement repairRequirement;
}

[CreateAssetMenu(menuName = "Spaecship Mechanic/RepairRecipe")]
public class RepairRecipe : ScriptableObject
{
    public int shipSectionID = -1;
    public int repairID = -1;

    public bool repairComplete { get; set; }

    [SerializeField]
    public string repairTitle;

    [SerializeField]
    public string repairDescription;

    [SerializeField]
    public RepairStage[] repairStages;

    public int currentRepairStage { get; set; }

    private void OnDestroy()
    {
        Debug.Log("Repair recipe destroyed");
    }

}
