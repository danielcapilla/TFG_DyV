using UnityEngine;

[CreateAssetMenu(fileName = "DatabaseCredentials", menuName = "Database/Credentials")]
public class DatabaseCredentialsSO : ScriptableObject
{
    [Header("Servidor")]
    public string url = "https://tfvj.etsii.urjc.es/";

    [Header("Credenciales de aplicacion")]
    public string appUsername = "TFGVCDC";
    public string appPassword = "2024TFGminijuegos";
}
