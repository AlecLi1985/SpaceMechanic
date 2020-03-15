using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using Random = UnityEngine.Random;

public class MainGame : MonoBehaviour
{
    public static MainGame instance;

    public static event Action ScanCompleteEvent;
    public static event Action CheckShipSectionRepairCompleteEvent;
    public static event Action CheckRepairCompleteEvent;
    public static event Action ResetRepairLoopEvent;

    public string hangarSound;
    public string plasmaTorchSound;
    public string sealantGunSound;
    public string scannerSound;
    public string scannerDeniedSound;
    public string repairHotspotClickSound;
    public string repairDeniedSound;

    public GameObject scannerAnimatorGroup;
    public Animator[] scannerAnimators;

    public CinemachineBrain cameraBrain;
    public CinemachineVirtualCamera hangarViewCam;
    public CinemachineVirtualCamera shipSectionViewCam;
    public CinemachineVirtualCamera cockpitViewCam;
    public CameraScript orbitCamera;
    [HideInInspector]
    public Camera currentCamera;

    public float shipSectionViewMinOrthoSize = 3f;
    public float shipSectionViewMaxOrthoSize = 8f;

    public float scanTotalTime;

    public GameObject repairHotspotObject;
    public Transform shipSectionPlaceholder;

    public List<GameObject> shipComponentObjects;
    public List<GameObject> damagedShipComponentObjects;
    public List<GameObject> damagedShipSectionObjects;

    //[HideInInspector]
    public GameObject shipSectionViewObject;
    //[HideInInspector]
    public ShipComponent shipSectionViewShipComponent;

    [HideInInspector]
    public float scanCurrentTime;

    public bool scanShip { get; set; }
    [HideInInspector]
    public bool canScanShip = true;

    //a list of currently owned tools, use this to populate the tools panel in the ship section view repair screen
    List<Tool> ownedTools;
    //a list of allcurrently owned parts, use this to populate the parts panel in the ship section view repair screen
    List<PartsAmount> ownedParts;

    //a temporary place to store the currently selected repair, updated everytime the player clicks on a repair hotspot.
    [HideInInspector]
    public RepairRecipe activeRepairRecipe;
    [HideInInspector]
    public RepairHotspot currentRepairHotspot;

    //a list of all repairs, added to when the player clicks on a repair hotspot for the first time.
    //when a repair stage is completed, the repair entry is updated to the next stage, the current repair is changed to reflect this
    //the corresponding currentTools and currentParts entries are emptied
    //when a repair is completed, the entry and corresponding entries in currentTools and currentParts are removed from the list.
    [Serializable]
    public class RepairList
    {
        public List<RepairRecipe> repairList;
        public RepairList()
        {
            repairList = new List<RepairRecipe>();
        }
    }
    [Serializable]
    public class RepairDictionary : SerializableDictionaryBase<int, RepairRecipe> { }
    [SerializeField]
    public RepairDictionary currentRepairs;

    //a list of lists of all tools added to the tools drop area in the repair box, index corresponds to the repair recipe in currentRepairs list
    //a new entry is automatically initialized when a repair is added to the current repairs list.
    [Serializable]
    public class ToolList
    {
        public List<Tool> toolList;
        public ToolList()
        {
            toolList = new List<Tool>();
        }

    }
    [Serializable]
    public class ToolDictionary : SerializableDictionaryBase<int, ToolList> { }
    [SerializeField]
    public ToolDictionary currentTools;

    //a list of lists of all parts and there amounts added to the drop area in the repair box, index correpsonds to the repair recipe in the currentRepairs list
    //a new entry is automatically initialized when a repair is added to the current repairs list.
    [Serializable]
    public class PartList
    {
        public List<PartsAmount> partList;
        public PartList()
        {
            partList = new List<PartsAmount>();
        }
    }
    [Serializable]
    public class PartDictionary : SerializableDictionaryBase<int, PartList> { }
    [SerializeField]
    public PartDictionary currentParts;

    bool isScanning = false;
    bool scanResultsGenerated = false;

    bool inShipSectionView = false;
    bool inHangarView = false;

    int shipSectionsDamaged = 0;
    int shipSectionsRepaired = 0;

    float difficulty = 0f;
    float easyMax = .3f;
    float mediumMax = .7f;

    public int easyShipSectionsDamagedMax = 3;
    public int mediumShipSectionsDamagedMax = 6;
    public int hardShipSectionsDamagedMax = 9;

    float damagedAreasDifficulty = 0f;
    float damagedAreasEasyMax = .4f;
    float damagedAreasMediumMax = .7f;

    int easyDamagedAreasMax = 2;
    int mediumDamagedAreasMax = 4;
    int hardDamagedAreasMax = 6;

    bool allowSecondaryShipSections = false;
    bool allowSystemsShipSections = false;
    bool allowInstrumentationShipSections = false;

    List<Vector3> randomPoints;
    float randomPointSpacing = 15f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        shipComponentObjects = new List<GameObject>();

    }

    // Start is called before the first frame update
    void Start()
    {
        damagedShipComponentObjects = new List<GameObject>();
        damagedShipSectionObjects = new List<GameObject>();

        randomPoints = new List<Vector3>();

        currentRepairs = new RepairDictionary();
        currentTools = new ToolDictionary();
        currentParts = new PartDictionary();

        scanShip = false;
        scanCurrentTime = 0f;

        inHangarView = true;
        inShipSectionView = false;

        scannerAnimators = scannerAnimatorGroup.GetComponentsInChildren<Animator>();
        foreach(Animator animator in scannerAnimators)
        {
            animator.gameObject.SetActive(false);
            animator.SetBool("scanEnabled", false);
        }

        SoundManager.instance.PlaySound(hangarSound);

        //main game doesn;t get destroyed so don;t have to chick for multiple subscription
        ShipComponent.SelectedDamagedShipComponentEvent += GenerateShipSectionObject;
        RepairHotspot.MouseOverRepairHotspotEnterEvent += OnRepairHotspotEnter;
        RepairHotspot.MouseOverRepairHotspotExitEvent += OnRepairHotspotExit;
        RepairHotspot.MouseOverRepairHotspotClickedEvent += OnRepairHotspotClicked;

        UsableIcon.OnToolIconUsed += AddToolsToCurrentRepairRecipe;
        UsableIcon.OnPartIconUsed += AddPartsToCurrentRepairRecipe;
        UsableIcon.OnToolIconRemoved += RemoveToolsFromCurrentRepairRecipe;
        UsableIcon.OnPartIconRemoved += RemovePartsFromCurrentRepairRecipe;

    }

    // Update is called once per frame
    void Update()
    {
        currentCamera = cameraBrain.OutputCamera;

        if (inHangarView)
        {
            if (scanShip && scanResultsGenerated == false)
            {
                ScanShip();
            }
        }

        if(inShipSectionView)
        {
            if (shipSectionViewCam.m_Priority == 1)
            {
                AdjustCameraOrthographicSize(Input.mouseScrollDelta.y * 0.25f);
            }

            //ShipSectionViewUpdate();
        }
    }

    void ScanShip()
    {
        scanCurrentTime += Time.deltaTime;

        if (scanCurrentTime > scanTotalTime)
        {
            scanCurrentTime = 0f;
            scanShip = false;

            GenerateScanResults();

            foreach (Animator animator in scannerAnimators)
            {
                animator.gameObject.SetActive(false);
                animator.SetBool("scanEnabled", false);
            }

            SoundManager.instance.StopSound(scannerSound);

            ScanCompleteEvent.Invoke();
        }
    }

    public void PlayScanShipSoundAndAnimation()
    {
        if(scanResultsGenerated == false && isScanning == false)
        {
            isScanning = true;
            SoundManager.instance.PlaySound(scannerSound);

            foreach (Animator animator in scannerAnimators)
            {
                animator.gameObject.SetActive(true);
                animator.SetBool("scanEnabled", true);
            }
        }
        else
        {
            SoundManager.instance.PlaySound(scannerDeniedSound);
        }
    }

    /// <summary>
    /// Hanager view update and methods
    /// </summary>

    void GenerateScanResults()
    {
        damagedShipComponentObjects.Clear();
        damagedShipSectionObjects.Clear();

        shipSectionsDamaged = 0;
        allowSecondaryShipSections = false;
        allowInstrumentationShipSections = false;

        difficulty = Random.value;

        if(difficulty <= easyMax)
        {
            shipSectionsDamaged = Random.Range(1, easyShipSectionsDamagedMax);
        }
        else if(difficulty > easyMax && difficulty <= mediumMax)
        {
            shipSectionsDamaged = Random.Range(mediumShipSectionsDamagedMax / 2, mediumShipSectionsDamagedMax);
            allowSecondaryShipSections = true;
        }
        else if(difficulty > mediumMax)
        {
            shipSectionsDamaged = Random.Range(hardShipSectionsDamagedMax - 2, hardShipSectionsDamagedMax);
            allowSecondaryShipSections = true;
            allowInstrumentationShipSections = true;
        }

        int currentShipSectionsAssignedDamage = 0;

        Debug.Log(shipSectionsDamaged);

        while(currentShipSectionsAssignedDamage < shipSectionsDamaged)
        {
            int randomId = Random.Range(0, shipComponentObjects.Count-1);
            ShipComponent shipComp;

            if (shipComponentObjects[randomId].TryGetComponent(out shipComp))
            {
                if (shipComp.isDamaged == false)
                {
                    ShipComponentType typeMask = ShipComponentType.PRIMARY;

                    if (allowSecondaryShipSections)
                    {
                        typeMask = ShipComponentType.PRIMARY | ShipComponentType.SECONDARY;
                    }
                    else if (allowSecondaryShipSections && allowInstrumentationShipSections)
                    {
                        typeMask = ShipComponentType.PRIMARY | ShipComponentType.SECONDARY | ShipComponentType.INSTRUMENTATION;

                    }

                    //Debug.Log(typeMask.ToString());
                   
                    if (typeMask.HasFlag(shipComp.sectionType))
                    {
                        shipComp.ID = shipComp.gameObject.GetInstanceID();
                        shipComp.SetDamaged();

                        damagedShipComponentObjects.Add(shipComp.gameObject);
                        currentShipSectionsAssignedDamage++;
                    }

                }
            }
        }

        canScanShip = false;
        scanResultsGenerated = true;
    }

    /// <summary>
    /// ShipSectionView update and methods
    /// </summary>
    void GenerateShipSectionObject(GameObject obj)
    {
        ShipComponent shipComp;
        if(obj.TryGetComponent(out shipComp))
        {
            if(shipComp.isDamaged)
            {
                if (shipComp.isInstanced == false)
                {
                    shipComp.isInstanced = true;

                    GameObject shipCompObj = Instantiate(shipComp.instanceObject, shipSectionPlaceholder);
                    ShipComponent shipCompObjShipComp = shipCompObj.AddComponent<ShipComponent>();
                    shipCompObjShipComp.ID = shipComp.ID;
                    shipCompObjShipComp.isSelectable = false;
                    shipCompObjShipComp.originalTransform = shipComp.transform;

                    shipCompObj.transform.rotation = Random.rotation;
                    shipCompObj.AddComponent<ObjectRotator>();

                    if (shipComp.isMirrored)
                    {
                        Vector3 localScale = shipCompObj.transform.localScale;
                        localScale.x = -localScale.x;
                        shipCompObj.transform.localScale = localScale;
                    }

                    shipSectionViewObject = shipCompObj;
                    shipSectionViewShipComponent = shipCompObjShipComp;
                    damagedShipSectionObjects.Add(shipCompObj);

                    GenerateShipSectionDamagedAreas(shipCompObj);
                }
                else
                {
                    foreach (GameObject shipCompObj in damagedShipSectionObjects)
                    {
                        ShipComponent shipCompObjComp = shipCompObj.GetComponent<ShipComponent>();
                       if (shipCompObjComp.ID == shipComp.ID)
                        {
                            shipSectionViewObject = shipCompObj;
                            shipSectionViewShipComponent = shipCompObjComp;
                            EnableShipSectionObject();
                        }
                    }
                }
            }
            else
            {
                Debug.Log("generate ship section failed - ship section not damaged");
            }
        }
        else
        {
            Debug.Log("generate ship section failed - no ship component");
        }
    }

    void GenerateShipSectionDamagedAreas(GameObject shipCompObj)
    {
        //will never been in a position to reinitialize an already initialized list of these so you dont have to check.
        shipSectionViewShipComponent.damagedAreas = new List<GameObject>();

        int areasDamaged = 0;
        damagedAreasDifficulty = Random.value;

        if (damagedAreasDifficulty <= damagedAreasEasyMax)
        {
            areasDamaged = Random.Range(1, easyDamagedAreasMax);

        }
        else if (damagedAreasDifficulty > damagedAreasEasyMax && damagedAreasDifficulty <= damagedAreasMediumMax)
        {
            areasDamaged = Random.Range(mediumDamagedAreasMax / 2, mediumDamagedAreasMax);
        }
        else if (damagedAreasDifficulty > damagedAreasMediumMax)
        {
            areasDamaged = Random.Range(hardDamagedAreasMax - 2, hardDamagedAreasMax);
        }

        for(int i = 0; i < areasDamaged; i++)
        {
            MeshFilter shipSectionMeshFilter = shipSectionViewObject.GetComponent<MeshFilter>();
            if (shipSectionMeshFilter != null)
            {
                Vector3 randomPoint = GetRandomPointOnMesh(shipSectionMeshFilter.mesh);
                bool searchForRandomPoint = true;

                while (searchForRandomPoint)
                {
                    if (randomPoints.Count == 0)
                    {
                        randomPoints.Add(randomPoint);
                        searchForRandomPoint = false;
                    }
                    else
                    {
                        bool randomPointValid = true;
                        foreach(Vector3 point in randomPoints)
                        {
                            if((point - randomPoint).magnitude < randomPointSpacing)
                            {
                                randomPointValid = false;
                                break;
                            }
                        }

                        if(randomPointValid == false)
                        {
                            randomPoint = GetRandomPointOnMesh(shipSectionMeshFilter.mesh);
                            searchForRandomPoint = true;
                        }
                        else
                        {
                            randomPoints.Add(randomPoint);
                            searchForRandomPoint = false;
                        }
                    }
                }

                GameObject damagedAreaObject = Instantiate(repairHotspotObject, shipCompObj.transform);
                damagedAreaObject.transform.localPosition = randomPoint;

                ShipComponent shipCompObjComp = shipCompObj.GetComponent<ShipComponent>();
                shipCompObjComp.damagedAreas.Add(damagedAreaObject);

                RepairHotspot hotspot = damagedAreaObject.GetComponent<RepairHotspot>();
                hotspot.repairRecipe.shipSectionID = shipCompObjComp.ID;
            }
            else
            {
                Debug.Log("failed to generate damaged area - mesh component not found");
            }
            randomPoints.Clear();
        }
    }

    void OnRepairHotspotEnter(RepairHotspot hotspot)
    {
        //Debug.Log("over a repair hotspot, call from MainGame");
    }

    void OnRepairHotspotExit(RepairHotspot hotspot)
    {
        //Debug.Log("exited a repair hotspot, call from MainGame");
    }

    void OnRepairHotspotClicked(RepairHotspot hotspot)
    {
        SoundManager.instance.PlaySound(repairHotspotClickSound);

        ClearToolsAndPartsLists();
        CheckShipSectionRepairCompleteEvent.Invoke();

        if (hotspot.repairRecipe.repairID == -1)
        {
            hotspot.repairRecipe.repairID = hotspot.repairRecipe.GetHashCode();

            currentRepairs.Add(hotspot.repairRecipe.repairID, hotspot.repairRecipe);
            activeRepairRecipe = hotspot.repairRecipe;

            currentTools.Add(hotspot.repairRecipe.repairID, new ToolList());
            currentParts.Add(hotspot.repairRecipe.repairID, new PartList());
        }
        else
        {
            activeRepairRecipe = currentRepairs[hotspot.repairRecipe.repairID];
        }

        currentRepairHotspot = hotspot;
    }

    void AddToolsToCurrentRepairRecipe(Tool tool)
    {
        if (activeRepairRecipe != null)
        {
            if (currentTools.ContainsKey(activeRepairRecipe.repairID))
            {
                currentTools[activeRepairRecipe.repairID].toolList.Add(tool);
            }
        }
    }

    void AddPartsToCurrentRepairRecipe(Part part, int amount)
    {
        if (activeRepairRecipe != null)
        {
            if (currentParts.ContainsKey(activeRepairRecipe.repairID))
            {
                PartsAmount parts;
                parts.part = part;
                parts.amount = amount;
                currentParts[activeRepairRecipe.repairID].partList.Add(parts);
            }
        }
    }

    void RemoveToolsFromCurrentRepairRecipe(Tool tool)
    {
        if(activeRepairRecipe != null)
        {
            if (currentTools.ContainsKey(activeRepairRecipe.repairID))
            {
                currentTools[activeRepairRecipe.repairID].toolList.Remove(tool);
            }
        }
        
    }

    void RemovePartsFromCurrentRepairRecipe(Part part, int amount)
    {
        if (activeRepairRecipe != null)
        {
            if (currentParts.ContainsKey(activeRepairRecipe.repairID))
            {
                PartsAmount parts;
                parts.part = part;
                parts.amount = amount;
                currentParts[activeRepairRecipe.repairID].partList.Remove(parts);
            }
        }
    }

    public void CheckRepairStageCompleted()
    {
        bool toolRequirementMet = false;
        foreach(Tool tool in currentTools[activeRepairRecipe.repairID].toolList)
        {
            if (activeRepairRecipe.repairStages[activeRepairRecipe.currentRepairStage].repairRequirement.tool == tool)
            {
                toolRequirementMet = true;
            }
            else
            {
                toolRequirementMet = false;
                break;
            }
        }

        bool partsRequirementMet = false;
        bool resume = false;
        if(activeRepairRecipe.repairStages[activeRepairRecipe.currentRepairStage].repairRequirement.partRequirements.Length == 0)
        {
            if (currentParts[activeRepairRecipe.repairID].partList.Count > 0)
            {
                partsRequirementMet = false;
            }
            else
            {
                partsRequirementMet = true;
            }
        }
        else
        {
            int partsTotal = 0;
            foreach (PartRequirement requirement in activeRepairRecipe.repairStages[activeRepairRecipe.currentRepairStage].repairRequirement.partRequirements)
            {
                foreach (PartsAmount parts in currentParts[activeRepairRecipe.repairID].partList)
                {
                    if (parts.part == requirement.parts.part)
                    {
                        partsTotal += parts.amount;
                        if (partsTotal >= requirement.parts.amount)
                        {
                            resume = true;
                        }
                    }
                    else
                    {
                        resume = false;
                        break;
                    }
                }

                if (resume)
                {
                    partsRequirementMet = true;
                    resume = false;
                    continue;
                }
                else
                {
                    partsRequirementMet = false;
                }
            }
        }

        if(toolRequirementMet && partsRequirementMet)
        {
            activeRepairRecipe.repairStages[activeRepairRecipe.currentRepairStage].repairRequirement.requirementMet = true;

            if(activeRepairRecipe.repairStages[activeRepairRecipe.currentRepairStage].repairRequirement.tool == Tool.PLASMA_TORCH)
            {
                SoundManager.instance.PlaySound(plasmaTorchSound);
            }
            else if(activeRepairRecipe.repairStages[activeRepairRecipe.currentRepairStage].repairRequirement.tool == Tool.SEALANT_GUN)
            {
                SoundManager.instance.PlaySound(sealantGunSound);
            }

            if (activeRepairRecipe.currentRepairStage < activeRepairRecipe.repairStages.Length - 1)
            {
                activeRepairRecipe.currentRepairStage++;

                ClearToolsAndPartsLists();

                if(CheckShipSectionRepairCompleteEvent != null)
                {
                    CheckShipSectionRepairCompleteEvent.Invoke();
                }

            }
            else
            {
                activeRepairRecipe.repairComplete = true;

                ClearAllDictionaries();

                shipSectionViewShipComponent.damagedAreas.Remove(currentRepairHotspot.gameObject);

                if (CheckShipSectionRepairCompleteEvent != null)
                {
                    CheckShipSectionRepairCompleteEvent.Invoke();
                }
            }

        }
        else
        {
            SoundManager.instance.PlaySound(repairDeniedSound);
        }

        if (activeRepairRecipe.repairComplete)
        {
            activeRepairRecipe = null;
            CheckShipSectionRepairCompleted();
        }

    }

    public void CheckShipSectionRepairCompleted()
    {
        bool shipSectionRepairComplete = true;
        foreach(GameObject damagedArea in shipSectionViewShipComponent.damagedAreas)
        {
            RepairHotspot hotspot = damagedArea.GetComponent<RepairHotspot>();
            if(hotspot.repairRecipe.repairComplete == false)
            {
                shipSectionRepairComplete = false;
                break;
            }
        }

        if(shipSectionRepairComplete)
        {
            //set damaged flag on ship componenet in the hangar view to false and the canhighlight flag to false
            shipSectionViewShipComponent.originalTransform.GetComponent<ShipComponent>().isDamaged = false;
            shipSectionViewShipComponent.originalTransform.GetComponent<MaterialSwitch>().canHighlight = false;

            //show something on the ship section view to declare that repair has been completed
            Debug.Log("Ship Section Repairs complete - well done!");
        }
    }

    public void CheckShipRepairComplete()
    {
        if(damagedShipComponentObjects.Count == 0)
        {
            Debug.Log("Ship Repairs complete - congratulations!");
            if(CheckRepairCompleteEvent != null)
            {
                CheckRepairCompleteEvent.Invoke();
            }
        }
    }

    public void ResetRepairLoop()
    {
        inHangarView = true;

        canScanShip = true;
        scanShip = false;
        scanResultsGenerated = false;
        isScanning = false;

        if(ResetRepairLoopEvent != null)
        {
            ResetRepairLoopEvent.Invoke();
        }
    }

    public void ClearToolsAndPartsLists()
    {
        if(activeRepairRecipe != null)
        {
            currentTools[activeRepairRecipe.repairID].toolList.Clear();
            currentParts[activeRepairRecipe.repairID].partList.Clear();
        }
    }

    public void ClearAllDictionaries()
    {
        if (activeRepairRecipe != null)
        {
            currentRepairs.Remove(activeRepairRecipe.repairID);
            currentTools.Remove(activeRepairRecipe.repairID);
            currentParts.Remove(activeRepairRecipe.repairID);
        }
    }

    public void EnableShipSectionObject()
    {
        if(shipSectionViewObject != null)
        {
            shipSectionViewObject.SetActive(true);
        }
    }


    public void DisableShipSectionObject()
    {
        if (shipSectionViewObject != null)
        {
            shipSectionViewObject.SetActive(false);
        }
    }

    public void DestroyShipSectionObject()
    {
        if (shipSectionViewObject != null)
        {
            damagedShipSectionObjects.Remove(shipSectionViewObject);
            damagedShipComponentObjects.Remove(shipSectionViewObject.GetComponent<ShipComponent>().originalTransform.gameObject);

            foreach(GameObject damagedAreaObject in shipSectionViewObject.GetComponent<ShipComponent>().damagedAreas)
            {
                RepairHotspot hotspot = damagedAreaObject.GetComponent<RepairHotspot>();
                Destroy(hotspot.repairRecipe);
            }

            Destroy(shipSectionViewObject);

            shipSectionViewShipComponent = null;
            shipSectionViewObject = null;
        }
    }

    public void EnableShipSectionViewRotation()
    {
        if (shipSectionViewObject != null)
        {
            ObjectRotator rotator = shipSectionViewObject.GetComponent<ObjectRotator>();
            rotator.canRotate = true;
        }
    }

    public void DisableShipSectionViewRotation()
    {
        if (shipSectionViewObject != null)
        {
            ObjectRotator rotator = shipSectionViewObject.GetComponent<ObjectRotator>();
            rotator.canRotate = false;
        }
    }

    public void SwitchToShipSectionScreenViewCam()
    {
        shipSectionViewCam.m_Priority = 1;
        hangarViewCam.m_Priority = 0;
        cockpitViewCam.m_Priority = 0;

        cameraBrain.OutputCamera.orthographic = true;
        shipSectionViewCam.m_Lens.OrthographicSize = 6;

        inShipSectionView = true;
        inHangarView = false;
    }

    public void SwitchToHangarViewCam()
    {
        shipSectionViewCam.m_Priority = 0;
        hangarViewCam.m_Priority = 1;
        cockpitViewCam.m_Priority = 0;

        cameraBrain.OutputCamera.orthographic = false;

        inShipSectionView = false;
        inHangarView = true;

        if (shipSectionViewShipComponent.originalTransform.GetComponent<ShipComponent>().isDamaged == false)
        {
            DestroyShipSectionObject();
        }

        CheckShipRepairComplete();
    }

    public void SwitchToCockpitViewCam()
    {
        shipSectionViewCam.m_Priority = 0;
        hangarViewCam.m_Priority = 0;
        cockpitViewCam.m_Priority = 1;

        cameraBrain.OutputCamera.orthographic = false;
    }

    public void AdjustCameraOrthographicSize(float delta)
    {
        shipSectionViewCam.m_Lens.OrthographicSize += delta;
        shipSectionViewCam.m_Lens.OrthographicSize = Mathf.Clamp(shipSectionViewCam.m_Lens.OrthographicSize, 
                                                                    shipSectionViewMinOrthoSize, 
                                                                    shipSectionViewMaxOrthoSize);
    }

    public void SetShipComponentsIgnoreRaycastLayer()
    {
        foreach (GameObject shipComp in shipComponentObjects)
        {
            shipComp.layer = 2;
        }
    }

    public void SetShipComponentsDefaultLayer()
    {
        foreach (GameObject shipComp in shipComponentObjects)
        {
            shipComp.layer = 0;
        }
    }

    void OnDestroy()
    {
        ShipComponent.SelectedDamagedShipComponentEvent -= GenerateShipSectionObject;
        RepairHotspot.MouseOverRepairHotspotEnterEvent -= OnRepairHotspotEnter;
        RepairHotspot.MouseOverRepairHotspotExitEvent -= OnRepairHotspotExit;
        RepairHotspot.MouseOverRepairHotspotClickedEvent -= OnRepairHotspotClicked;

        UsableIcon.OnToolIconUsed -= AddToolsToCurrentRepairRecipe;
        UsableIcon.OnPartIconUsed -= AddPartsToCurrentRepairRecipe;
        UsableIcon.OnToolIconRemoved -= RemoveToolsFromCurrentRepairRecipe;
        UsableIcon.OnPartIconRemoved -= RemovePartsFromCurrentRepairRecipe;
    }

    //from https://gist.github.com/v21/5378391
    Vector3 GetRandomPointOnMesh(Mesh mesh)
    {
        Vector3 Result = Vector3.zero;

        //if you're repeatedly doing this on a single mesh, you'll likely want to cache cumulativeSizes and total
        float[] sizes = GetTriSizes(mesh.triangles, mesh.vertices);
        float[] cumulativeSizes = new float[sizes.Length];
        float total = 0;

        for (int i = 0; i < sizes.Length; i++)
        {
            total += sizes[i];
            cumulativeSizes[i] = total;
        }

        //so everything above this point wants to be factored out

        float randomsample = Random.value * total;

        int triIndex = -1;

        for (int i = 0; i < sizes.Length; i++)
        {
            if (randomsample <= cumulativeSizes[i])
            {
                triIndex = i;
                break;
            }
        }

        if (triIndex == -1) Debug.LogError("triIndex should never be -1");

        Vector3 a = mesh.vertices[mesh.triangles[triIndex * 3]];
        Vector3 b = mesh.vertices[mesh.triangles[triIndex * 3 + 1]];
        Vector3 c = mesh.vertices[mesh.triangles[triIndex * 3 + 2]];

        //generate random barycentric coordinates

        float r = Random.value;
        float s = Random.value;

        if (r + s >= 1)
        {
            r = 1 - r;
            s = 1 - s;
        }
        //and then turn them back to a Vector3
        Result = a + r * (b - a) + s * (c - a);

        return Result;
    }

    float[] GetTriSizes(int[] tris, Vector3[] verts)
    {
        int triCount = tris.Length / 3;
        float[] sizes = new float[triCount];
        for (int i = 0; i < triCount; i++)
        {
            sizes[i] = .5f * Vector3.Cross(verts[tris[i * 3 + 1]] - verts[tris[i * 3]], verts[tris[i * 3 + 2]] - verts[tris[i * 3]]).magnitude;
        }
        return sizes;
    }


}
