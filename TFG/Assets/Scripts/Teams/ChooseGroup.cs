using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChooseGroup : NetworkBehaviour
{
    PlayerStats player;
    [SerializeField] TeamMenager teamManager;
    [SerializeField] Button readyButton;
    [SerializeField] private GameObject groupCanvas;
    [SerializeField] private Countdown countdown;
    private Dictionary<ulong, bool> playerReadyDictionary;
    public static List<ulong> connectedPlayers;
    [SerializeField]
    private PlayerSpawner playerSpawner;
    [SerializeField]
    private CameraSelector cameraSelector;

    private bool host = false;
    private Button previousButton;
    private Button[] buttons;

    public delegate void PlayerReady(ulong id);
    public event PlayerReady OnPlayerReady;

    [SerializeField] AudioSource restaurantMusic;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        readyButton.gameObject.SetActive(false);
        buttons = GetComponentsInChildren<Button>();

        if (IsServer)
        {

            playerReadyDictionary = new Dictionary<ulong, bool>();
            connectedPlayers = NetworkManager.Singleton.ConnectedClientsIds.ToList<ulong>();
            if (host) return;
            groupCanvas.gameObject.SetActive(false);
            connectedPlayers.Remove(OwnerClientId);

        }

    }
    public void ChangeGroup()
    {
        if (previousButton != null)
        {
            previousButton.interactable = true;
        }
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        clickedButton.interactable = false;
        previousButton = clickedButton;
        ChangeGroupRPC(NetworkManager.Singleton.LocalClientId, (int.Parse(clickedButton.GetComponentInChildren<TextMeshProUGUI>().text)) - 1);
        readyButton.gameObject.SetActive(true);
    }
    [Rpc(SendTo.Server)]
    private void ChangeGroupRPC(ulong id, int groupNumber)
    {
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponent<PlayerStats>();
        player.idGrupo.Value = groupNumber;

    }
    public void ReadyPlayer()
    {
        readyButton.interactable = false;
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }
        OnPlayerReady?.Invoke(NetworkManager.Singleton.LocalClientId);
        ReadyPlayerRPC(NetworkManager.Singleton.LocalClientId);

    }
    [Rpc(SendTo.Server)]
    public void ReadyPlayerRPC(ulong id)
    {
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponent<PlayerStats>();
        TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamManager.teams[player.idGrupo.Value];
        teamInfo.integrantes.Add(id);
        SetPlayerReady(id);
    }

    public void SetPlayerReady(ulong id)
    {
        playerReadyDictionary[id] = true;
        bool allClientsReady = true;
        foreach (ulong clientId in connectedPlayers)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }
        if (allClientsReady)
        {
            //playerSpawner.InstantiatePlayer();
            DesactivateGroupCanvasRPC();
            countdown.CambiarVariable();
            foreach (ulong playerId in connectedPlayers)
            {
                //cameraSelector.ActivateCamera(NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerStats>().idGrupo.Value);
                SetCameraRPC(NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInParent<PlayerStats>().idGrupo.Value, playerId);
                PlayerInput playerInput = NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerInput>();
                ActivatePlayerInputRPC(playerInput.GetComponent<NetworkObject>());
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ActivatePlayerInputRPC(NetworkObjectReference playerInputNetworkObjectReference)
    {
        playerInputNetworkObjectReference.TryGet(out NetworkObject playerInputNetworkObject);
        PlayerController playerController = playerInputNetworkObject.GetComponent<PlayerController>();
        playerController.enabled = true;
    }
    [Rpc(SendTo.Everyone)]
    public void DesactivateGroupCanvasRPC()
    {
        groupCanvas.gameObject.SetActive(false);
        restaurantMusic.Play();
    }
    [Rpc(SendTo.Everyone)]
    private void SetCameraRPC(int groupID, ulong id)
    {
        if (id != NetworkManager.Singleton.LocalClientId) return;
        cameraSelector.ActivateCamera(groupID);
    }
}
