using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileBehaviour : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image image;

    ProfileImageList imageList;
    [SerializeField] DataBaseCommander commander;

    [SerializeField] GameObject ProfilePicButtonPrefab;
    [SerializeField] GridLayoutGroup gridLayout;

    // Start is called before the first frame update
    void Start()
    {
        imageList = FindAnyObjectByType<ProfileImageList>();
        text.text = PlayerData.Name;
        image.sprite = imageList.ProfilePics[PlayerData.ProfilePicID];

        for (int i = 0; i < imageList.ProfilePics.Count; i++)
        {
            GameObject Instance = Instantiate(ProfilePicButtonPrefab);
            Instance.GetComponentsInChildren<Image>()[1].sprite = imageList.ProfilePics[i];
            int tmp = i;
            Instance.GetComponent<Button>().onClick.AddListener(() => { ChangeProfilePic(tmp); });
            Instance.transform.SetParent(gridLayout.transform, false);
        }
    }

    public void ChangeProfilePic(int id)
    {
        PlayerData.ProfilePicID = id;
        image.sprite = imageList.ProfilePics[PlayerData.ProfilePicID];
    }

    public void CloseProfilePicSelector()
    {
        //Update data in DB
        commander.UpdateGame();
    }
}
