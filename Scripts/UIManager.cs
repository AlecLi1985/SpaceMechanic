using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject hangarUI;
    public GameObject shipSectionUI;
    public string sectionViewOpenSound;
    public string sectionViewCloseSound;

    // Start is called before the first frame update
    void Start()
    {
        hangarUI.SetActive(true);
        shipSectionUI.SetActive(false);

        ShipComponent.SelectedDamagedShipComponentEvent += HangarToShipSectionSwitch;
    }

    public void HangarToShipSectionSwitch(GameObject obj)
    {

        hangarUI.SetActive(false);
        shipSectionUI.SetActive(true);

        MainGame.instance.SetShipComponentsIgnoreRaycastLayer();

        MainGame.instance.orbitCamera.isEnabled = false;
        MainGame.instance.SwitchToShipSectionScreenViewCam();

        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        SoundManager.instance.PlaySound(sectionViewOpenSound);
    }

    public void ShipSectionToHangarSwitch()
    {

        hangarUI.SetActive(true);
        shipSectionUI.SetActive(false);

        MainGame.instance.SetShipComponentsDefaultLayer();

        MainGame.instance.orbitCamera.isEnabled = true;
        MainGame.instance.SwitchToHangarViewCam();

        SoundManager.instance.PlaySound(sectionViewCloseSound);

    }

    void OnDestroy()
    {
        ShipComponent.SelectedDamagedShipComponentEvent -= HangarToShipSectionSwitch;
    }
}
