using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkStarter : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button startGameButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
        startGameButton.onClick.AddListener(StartGame);
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
    private void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Loading GameScene for all players...");
            NetworkManager.Singleton.SceneManager.LoadScene("Grid", LoadSceneMode.Single);
        }
    }
}
