using TMPro;
using UnityEngine;

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
    TMP_InputField classCodeRegister;
    [SerializeField]
    TextMeshProUGUI ageRegisterText;
    private int age = 5;
    [SerializeField] GameObject usernameRegisterPanel;
    [SerializeField] GameObject ageRegisterPanel;
    [SerializeField] GameObject roleRegisterPanel;
    [SerializeField] GameObject GenderRegisterPanel;
    [SerializeField] TextMeshProUGUI usernameRegisterErrorText;
    private bool isGenderSet = false;
    private bool isRoleSet = false;
    [SerializeField] TextMeshProUGUI genderRegisterErrorText;
    [SerializeField] TextMeshProUGUI roleRegisterErrorText;

    [SerializeField] DataBaseCommander commander;


    private void Start()
    {
        ageRegisterText.text = age.ToString();
    }

    public void Login()
    {
        if (usernameLogin.text.Length > 0)
        {
            //Guardar info en la clase statica
            commander.LoadGame(usernameLogin.text);
        }
        else
        {
            //Poner en rojo y avisar de que hay que introducir info en los campos
            errorText.gameObject.SetActive(true);
        }
    }


    public void RegisterUsername()
    {
        if (usernameRegister.text.Length > 0 && classCodeRegister.text.Length > 0)
        {
            PlayerData.Name = usernameRegister.text;
            PlayerData.ClassCode = classCodeRegister.text;
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


    public void SetRole(int role)
    {
        if (role == 0)
        {
            PlayerData.Role = "Teacher";
        }
        else if (role == 1)
        {
            PlayerData.Role = "Student";
        }
        isRoleSet = true;
    }

    public void AdvanceToGenderSelector()
    {
        if (isRoleSet)
        {
            roleRegisterPanel.SetActive(false);
            GenderRegisterPanel.SetActive(true);
        }
        else
        {
            roleRegisterErrorText.gameObject.SetActive(true);
        }
    }

    public void RegisterPlayer()
    {
        if (isGenderSet)
        {
            //Register

            commander.RegisterUser();
        }
        else
        {
            genderRegisterErrorText.gameObject.SetActive(true);
        }
    }
}
