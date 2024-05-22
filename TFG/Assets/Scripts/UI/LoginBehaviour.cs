using TMPro;
using UnityEngine;

public class LoginBehaviour : MonoBehaviour
{
    [Header("Login")]
    [SerializeField]
    TMP_InputField usernameLogin;

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

    private bool isGenderSet = false;
    private bool isRoleSet = false;
    [Header("Error Texts")]
    [SerializeField] TextMeshProUGUI MissingFieldsErrorText;
    [SerializeField] TextMeshProUGUI UsernameRegisterErrorText;
    [SerializeField] TextMeshProUGUI GenderRegisterErrorText;
    [SerializeField] TextMeshProUGUI RoleRegisterErrorText;
    [SerializeField] TextMeshProUGUI DBErrorText;
    [SerializeField] TextMeshProUGUI UserNotFoundErrorText;
    [SerializeField] TextMeshProUGUI UserAlreadyExistsErrorText;

    [SerializeField] DataBaseCommander commander;


    private void Start()
    {
        ageRegisterText.text = age.ToString();
        commander.DataBaseLogin();
    }

    public void HideErrorTexts()
    {
        MissingFieldsErrorText.gameObject.SetActive(false);
        UsernameRegisterErrorText.gameObject.SetActive(false);
        UserAlreadyExistsErrorText.gameObject.SetActive(false);
        GenderRegisterErrorText.gameObject.SetActive(false);
        RoleRegisterErrorText.gameObject.SetActive(false);
        DBErrorText.gameObject.SetActive(false);
        UserNotFoundErrorText.gameObject.SetActive(false);
        UserAlreadyExistsErrorText.gameObject.SetActive(false);
    }

    public void Login()
    {
        HideErrorTexts();
        if (usernameLogin.text.Length > 0)
        {
            //Guardar info en la clase statica
            commander.LoadGame(usernameLogin.text, LoginError);
        }
        else
        {
            //Poner en rojo y avisar de que hay que introducir info en los campos
            MissingFieldsErrorText.gameObject.SetActive(true);
        }
    }

    public void LoginError(int error)
    {
        if (error == 0)
        {
            //Error lo que has buscado no existe
            Debug.Log("No hay usuario");
            UserNotFoundErrorText.gameObject.SetActive(true);
        }
        else if (error == 1)
        {
            //La BD ha petado por algo
            DBErrorText.gameObject.SetActive(true);
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
            UsernameRegisterErrorText.gameObject.SetActive(true);
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
            RoleRegisterErrorText.gameObject.SetActive(true);
        }
    }

    public void RegisterPlayer()
    {
        HideErrorTexts();
        if (isGenderSet)
        {
            //Register
            commander.RegisterUser(RegisterError);
        }
        else
        {
            GenderRegisterErrorText.gameObject.SetActive(true);
        }
    }

    public void RegisterError(int error)
    {
        if (error == 0)
        {
            //Este usuario ya existe
            Debug.Log("Este usuario ya existe");
            UserAlreadyExistsErrorText.gameObject.SetActive(true);
        }
        else if (error == 1)
        {
            //Other unkown error
            DBErrorText.gameObject.SetActive(true);
        }
    }
}
