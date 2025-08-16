using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserNetworkConfig : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> usernameNetworkVariable = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> profilePicIDNetworkVariable = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private bool conectionFailed = false;
    public override void OnNetworkSpawn()
    {
        //NetworkManager.Singleton.OnServerStopped += OnServerDisconnect;
        if (!IsOwner) return;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnServerDisconnect;

        usernameNetworkVariable.OnValueChanged += UpdateName;
        usernameNetworkVariable.Value = PlayerData.Name;
        profilePicIDNetworkVariable.OnValueChanged += UpdateProfilePic;
        profilePicIDNetworkVariable.Value = PlayerData.ProfilePicID;
    }

    private void OnServerDisconnect(ulong obj)
    {
        //Cuando el servidor se manda un mensaje como que es el cliente el que se ha desconectadoo...
        //Ya que es el cliente el que ha perdido la conexion con el servidor...
        //Por eso se tiene uqe preguntar si ha sido el que se ha desconectado para volver al menu
        if (obj == OwnerClientId)
        {
            MainMenuManager.IsError = true;
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
            SceneManager.LoadScene("MainMenu");

        }

    }

    private void OnServerDisconnect(bool obj)
    {
        Debug.Log("Server stopped");
        conectionFailed = true;
        //SceneManager.LoadScene("MainMenu");
    }

    private void UpdateProfilePic(int previousValue, int newValue)
    {
        profilePicIDNetworkVariable.Value = newValue;
    }

    private void UpdateName(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        usernameNetworkVariable.Value = newValue;
        Debug.Log("User: " + usernameNetworkVariable.Value);
    }

    public override void OnNetworkDespawn()
    {


        if (!IsOwner) return;
        usernameNetworkVariable.OnValueChanged -= UpdateName;
        NetworkManager.Singleton.OnServerStopped -= OnServerDisconnect;


    }
}
