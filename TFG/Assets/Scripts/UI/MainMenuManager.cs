using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI errorText;
    [SerializeField]
    private GameObject ClassInfoGO;

    public static bool IsError = false;

    private void Start()
    {
        if (IsError)
        {
            StartCoroutine(ShowErrorText(3));
        }
        if(PlayerData.Role != "Teacher")
        {
            ClassInfoGO.SetActive(false);
        }
    }
    private IEnumerator ShowErrorText(int seconds)
    {
        errorText.text = "Se ha desconectado de la sala.";
        yield return new WaitForSeconds(seconds);
        IsError = false;
        errorText.text = ""; 
    }

    public void GetClassInfoScene()
    {
        SceneManager.LoadScene("ClassInfo");
    }
}
