using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowButtons : MonoBehaviour
{
    [SerializeField]
    private GameObject hostButton;
    [SerializeField]
    private GameObject joinButton;
    [SerializeField]
    private GameObject input;

    void Start()
    {
        if(PlayerData.Role == "Teacher")
        {
            hostButton.SetActive(true);
            joinButton.SetActive(false);
            input.SetActive(false);
        }
        else
        {
            hostButton.SetActive(false);
            joinButton.SetActive(true);
            input.SetActive(true);
        }
    }

}
