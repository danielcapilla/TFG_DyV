using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginBehaviour : MonoBehaviour
{
    [SerializeField]
    TMP_InputField input;
    public void Login() 
    {
        if (input.text.Length > 0)
        {
            //Guardar info en la clase statica
            PlayerData.Name = input.text;
            SceneManager.LoadScene("MainMenu");
        }
        else 
        {
            //Poner en rojo y avisar de que hay que introducir info en los campos
        }
    }
}
