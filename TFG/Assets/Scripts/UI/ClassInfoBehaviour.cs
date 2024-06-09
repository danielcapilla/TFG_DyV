using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClassInfoBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject[] menus;
    
    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void GoBack()
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].gameObject.activeInHierarchy)
            {
                if (i == 0)
                {
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    menus[i - 1].gameObject.SetActive(true);
                    menus[i].gameObject.SetActive(false);
                }
                break;
            }
        }

    }
}
