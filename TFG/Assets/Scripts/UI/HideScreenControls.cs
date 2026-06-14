using UnityEngine;

public class HideScreenControls : MonoBehaviour
{
    [SerializeField] GameObject stickBackground;
    [SerializeField] GameObject stick;
    [SerializeField] GameObject interactButton;
    [SerializeField] GameObject interactButton2;

    [SerializeField] bool showControls = false;
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerData.Role == "Teacher") return;
        if (SystemInfo.deviceType == DeviceType.Handheld || Application.isMobilePlatform || showControls)
        {
            stickBackground.SetActive(true);
            stick.SetActive(true);
            interactButton.SetActive(true);
            interactButton2.SetActive(true);
        }
        else
        {
            stickBackground.SetActive(false);
            stick.SetActive(false);
            interactButton.SetActive(false);
            interactButton2.SetActive(false);
        }
    }

}
