using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI errorText;

    public static bool IsError = false;

    private void Start()
    {
        if (IsError)
        {
            StartCoroutine(ShowErrorText(3));

        }
    }
    private IEnumerator ShowErrorText(int seconds)
    {
        errorText.text = "El servidor se ha desconectado.";
        yield return new WaitForSeconds(seconds);
        IsError = false;
        errorText.text = ""; 
    }
}
