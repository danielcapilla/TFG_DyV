using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkStarter : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started");
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client started");
    }
}
