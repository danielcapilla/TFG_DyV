using UnityEngine;

public class HideScreenControls : MonoBehaviour
{
    [SerializeField] GameObject stick;
    [SerializeField] GameObject interactButton;
    // Start is called before the first frame update
    void Start()
    {
        if (SystemInfo.deviceType == DeviceType.Handheld || Application.isMobilePlatform)
        {
            stick.SetActive(true);
            interactButton.SetActive(true);
        }
        else
        {
            stick.SetActive(false);
            interactButton.SetActive(false);
        }
    }

}
