using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HangarUI : MonoBehaviour
{
    public GameObject titlePanel;
    public string repairCompleteSound;

    public GameObject repairRequestPanel;
    public TMP_Text feeAmountRequestText;
    public TMP_Text feeCurrencyRequestText;
    public TMP_Text customerCommentsRequestText;

    public Button scanButton;
    public TMP_Text scanButtonText;
    public string scanButtonLabel;
    public string scanButtonScanningLabel;
    public TMP_Text scanNotificationText;

    public Slider scanProgressSlider;

    public GameObject damagedSectionsNotificationPanel;
    public CanvasGroup damagedSectionNotificationCanvasGroup;
    public TMP_Text damagedSectionsNumberText;
    public TMP_Text damagedSectionsDetectedText;

    public GameObject repairCompletePanel;
    public TMP_Text feeAmountText;
    public TMP_Text feeCurrencyText;
    public TMP_Text customerCommentsText;

    public float fadeOutSpeed = 10f;
    bool fadeOutNotificationPanel = false;

    // Start is called before the first frame update
    void Start()
    {
        ShipComponent.SelectedDamagedShipComponentEvent += StopDamagedSectionNotificationCoroutine;

        MainGame.ScanCompleteEvent += ScanComplete;
        MainGame.CheckRepairCompleteEvent += ShowRepairCompletePanel;
        MainGame.CheckRepairCompleteEvent += HideScanButton;
        MainGame.CheckRepairCompleteEvent += HideTitle;

        scanProgressSlider.gameObject.SetActive(false);
        scanProgressSlider.value = 0f;

        scanNotificationText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (MainGame.instance.scanShip && MainGame.instance.canScanShip)
        {
            scanButton.interactable = false;
            scanButtonText.text = scanButtonScanningLabel;

            scanProgressSlider.gameObject.SetActive(true);
            scanProgressSlider.value = (1f / MainGame.instance.scanTotalTime) * MainGame.instance.scanCurrentTime;
        }

        if(fadeOutNotificationPanel)
        {
            damagedSectionNotificationCanvasGroup.alpha = Mathf.Lerp(damagedSectionNotificationCanvasGroup.alpha, 0f, fadeOutSpeed * Time.deltaTime);

            if(damagedSectionNotificationCanvasGroup.alpha <= 0f)
            {
                fadeOutNotificationPanel = false;
            }
        }
    }

    public void OnClickScanButton()
    {
        if(MainGame.instance.canScanShip == false)
        {
            StartCoroutine(ShowScanNotification());
        }
    }

    public void ShowScanButton()
    {
        scanButton.gameObject.SetActive(true);
    }

    public void HideScanButton()
    {
        scanButton.gameObject.SetActive(false);
    }

    public void ShowTitle()
    {
        titlePanel.SetActive(true);
    }

    public void HideTitle()
    {
        titlePanel.SetActive(false);
    }

    IEnumerator ShowScanNotification()
    {
        scanButton.interactable = false;
        scanNotificationText.gameObject.SetActive(true);

        StartCoroutine(ShowDamagedSectionsDetectedNotification());

        yield return new WaitForSeconds(1.0f);

        scanButton.interactable = true;
        scanNotificationText.gameObject.SetActive(false);
    }

    void ScanComplete()
    {
        scanButton.interactable = true;
        scanButtonText.text = scanButtonLabel;

        scanProgressSlider.gameObject.SetActive(false);
        scanProgressSlider.value = 0f;

        ShowDamagedSectionsDetectedNotificationCallback();
    }

    public void ShowDamagedSectionsDetectedNotificationCallback()
    {
        if (MainGame.instance.canScanShip == false)
        {
            StopCoroutine(ShowDamagedSectionsDetectedNotification());
            StartCoroutine(ShowDamagedSectionsDetectedNotification());
        }
    }

    public IEnumerator ShowDamagedSectionsDetectedNotification()
    {
        damagedSectionsNotificationPanel.SetActive(true);

        damagedSectionNotificationCanvasGroup.alpha = 1f;
        fadeOutNotificationPanel = false;

        int damagedSections = MainGame.instance.damagedShipComponentObjects.Count;
        damagedSectionsNumberText.text = damagedSections.ToString();
        
        if(damagedSections == 1)
        {
            damagedSectionsDetectedText.text = "Damaged Section Detected";
        }
        else
        {
            damagedSectionsDetectedText.text = "Damaged Sections Detected";
        }

        yield return new WaitForSeconds(1f);

        fadeOutNotificationPanel = true;
    }

    public void HideDamagedSectionsDetectedNotification()
    {
        damagedSectionsNotificationPanel.gameObject.SetActive(false);
    }

    void StopDamagedSectionNotificationCoroutine(GameObject obj)
    {
        StopCoroutine(ShowScanNotification());
        scanButton.interactable = true;
        scanNotificationText.gameObject.SetActive(false);

        StopCoroutine(ShowDamagedSectionsDetectedNotification());
        damagedSectionNotificationCanvasGroup.alpha = 0f;
        fadeOutNotificationPanel = false;
    }

    void ShowRepairCompletePanel()
    {
        SoundManager.instance.PlaySound(repairCompleteSound);

        repairCompletePanel.SetActive(true);
        //work out how this is calculated
        int fee = Random.Range(1, 10000);
        feeAmountText.text = fee.ToString();
        feeCurrencyText.text = Statics.alienRaces[Random.Range(0, Statics.alienRaces.Length - 1)] + " " + Statics.alienCurrencies[Random.Range(0, Statics.alienCurrencies.Length - 1)] + 
            (fee > 1 ? "s" : "");
        customerCommentsText.text = Statics.alienRepairCompleteComments[Random.Range(0, Statics.alienRaces.Length - 1)];
    }

    public void HideRepairCompletePanel()
    {
        repairCompletePanel.SetActive(false);
        feeAmountText.text = "";
        feeCurrencyText.text = "";
        customerCommentsText.text = "";
    }

    public void ShowCustomerRepairRequestPanel()
    {
        repairRequestPanel.SetActive(true);
        //work out how this is calculated
        int fee = Random.Range(1, 10000);
        feeAmountRequestText.text = fee.ToString();
        feeCurrencyRequestText.text = Statics.alienRaces[Random.Range(0, Statics.alienRaces.Length - 1)] + " " + Statics.alienCurrencies[Random.Range(0, Statics.alienCurrencies.Length - 1)] +
            (fee > 1 ? "s" : "");
        customerCommentsRequestText.text = Statics.alienRepairCompleteComments[Random.Range(0, Statics.alienRaces.Length - 1)];
    }

    public void HideCustomerRepairRequestPanel()
    {
        repairRequestPanel.gameObject.SetActive(false);
        feeAmountRequestText.text = "";
        feeCurrencyRequestText.text = "";
        customerCommentsRequestText.text = "";
    }

    void OnDestroy()
    {
        ShipComponent.SelectedDamagedShipComponentEvent -= StopDamagedSectionNotificationCoroutine;
        MainGame.ScanCompleteEvent -= ScanComplete;
        MainGame.CheckRepairCompleteEvent -= ShowRepairCompletePanel;
        MainGame.CheckRepairCompleteEvent -= HideScanButton;
        MainGame.CheckRepairCompleteEvent -= HideTitle;
    }
}
