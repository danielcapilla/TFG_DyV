using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileBehaviour : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image image;

    ProfileImageList imageList;
    [SerializeField] DataBaseCommander commander;


    // Start is called before the first frame update
    void Start()
    {
        imageList = FindAnyObjectByType<ProfileImageList>();
        text.text = PlayerData.Name;
        image.sprite = imageList.ProfilePics[PlayerData.ProfilePicID];
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
