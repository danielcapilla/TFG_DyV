using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginBehaviour : MonoBehaviour
{
    [Header("Login")]
    [SerializeField]
    TMP_InputField usernameLogin;
    [SerializeField] TextMeshProUGUI errorText;

    [Header("Register")]
    [SerializeField]
    TMP_InputField usernameRegister;
    [SerializeField]
    TextMeshProUGUI ageRegisterText;
    private int age = 5;
    [SerializeField] GameObject usernameRegisterPanel;
    [SerializeField] GameObject ageRegisterPanel;
    [SerializeField] TextMeshProUGUI usernameRegisterErrorText;
    private bool isGenderSet = false;
    [SerializeField] TextMeshProUGUI genderRegisterErrorText;


    private void Start()
    {
        ageRegisterText.text = age.ToString();
    }

    public void Login() 
    {
        if (usernameLogin.text.Length > 0)
        {
            //Guardar info en la clase statica
            PlayerData.Name = usernameLogin.text;
            SceneManager.LoadScene("MainMenu");
        }
        else 
        {
            //Poner en rojo y avisar de que hay que introducir info en los campos
            errorText.gameObject.SetActive(true);
        }
    }


    public void RegisterUsername()
    {
        if (usernameRegister.text.Length > 0)
        {
            PlayerData.Name = usernameRegister.text;
            usernameRegisterPanel.SetActive(false);
            ageRegisterPanel.SetActive(true);
        }
        else 
        {
            usernameRegisterErrorText.gameObject.SetActive(true);
        }
    }

    public void ChangeAge(int amount) 
    {
        age += amount;
        PlayerData.Age = age;
        ageRegisterText.text = age.ToString();
    }

    public void SetGender(int gender) 
    {
        if (gender == 0)
        {
            PlayerData.Gender = "Female";
        }
        else if (gender == 1) 
        {
            PlayerData.Gender = "Male";
        }
        isGenderSet = true;
    }

    public void RegisterPlayer() 
    {
        if (isGenderSet)
        {
            //Register
            SceneManager.LoadScene("MainMenu");
        }
        else 
        {
            genderRegisterErrorText.gameObject.SetActive(true);
        }
    }
}
