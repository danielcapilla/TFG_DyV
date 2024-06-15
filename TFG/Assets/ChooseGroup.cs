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
    [SerializeField]
    private Countdown countdown;
    private Dictionary<ulong, bool> playerReadyDictionary;
    public static List<ulong> connectedPlayers;
    [SerializeField]
    private PlayerSpawner playerSpawner;
    [SerializeField]
    private CameraSelector cameraSelector;
    [SerializeField]
    private RestaurantBehaviour[] restaurantBehaviourArray;
    private bool host = true;
    private Button previousButton;
    private Button[] buttons;

    [SerializeField] AudioSource restaurantMusic;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        readyButton.gameObject.SetActive(false);
        buttons = GetComponentsInChildren<Button>();
        //restaurantBehaviourArray = FindObjectsOfType<RestaurantBehaviour>();
        //Array.Sort(restaurantBehaviourArray);
        if (IsServer)
        {

            playerReadyDictionary = new Dictionary<ulong, bool>();
            connectedPlayers = NetworkManager.Singleton.ConnectedClientsIds.ToList<ulong>();
            if (host) return;
            this.gameObject.SetActive(false);
            connectedPlayers.Remove(OwnerClientId);

        }

    }
    public void Cambio()
    {
        if (previousButton != null)
        {
            previousButton.interactable = true;
        }
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        clickedButton.interactable = false;
        previousButton = clickedButton;
        CambioServerRPC(NetworkManager.Singleton.LocalClientId, (int.Parse(clickedButton.GetComponentInChildren<TextMeshProUGUI>().text)) - 1);
        readyButton.gameObject.SetActive(true);
    }
    [ServerRpc(RequireOwnership = false)]
    private void CambioServerRPC(ulong id, int groupNumber)
    {
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponentInChildren<PlayerStats>();
        SetPlayerPositionPart1ClientRPC(player.NetworkObject);
        player.idGrupo.Value = groupNumber;
        SetPlayerPositionPart2ClientRPC(groupNumber, player.NetworkObject);

    }
    public void ReadyPlayer()
    {
        readyButton.interactable = false;
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }
        ReadyPlayerServerRPC(NetworkManager.Singleton.LocalClientId);

    }
    [ServerRpc(RequireOwnership = false)]
    public void ReadyPlayerServerRPC(ulong id)
    {
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponentInChildren<PlayerStats>();
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
            DesactivateGroupCanvasClientRPC();
            countdown.CambiarVariable();
            foreach (ulong playerId in connectedPlayers)
            {
                //cameraSelector.ActivateCamera(NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerStats>().idGrupo.Value);
                SetCameraClientRPC(NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerStats>().idGrupo.Value, playerId);
                PlayerInput playerInput = NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerInput>();
                ActivatePlayerInputClientRPC(playerInput.GetComponent<NetworkObject>());
            }
        }
    }
    [ClientRpc]
    private void SetPlayerPositionPart1ClientRPC(NetworkObjectReference playerStatsNetworkObjectReference)
    {
        playerStatsNetworkObjectReference.TryGet(out NetworkObject playerStatsNetworkObject);
        PlayerStats player = playerStatsNetworkObject.GetComponent<PlayerStats>();
        if (player.idGrupo.Value != -1)
        {
            restaurantBehaviourArray[player.idGrupo.Value].RemovePosition(player.transform, player.OwnerClientId);
        }
    }
    [ClientRpc]
    private void SetPlayerPositionPart2ClientRPC(int groupNumber, NetworkObjectReference playerStatsNetworkObjectReference)
    {
        playerStatsNetworkObjectReference.TryGet(out NetworkObject playerStatsNetworkObject);
        PlayerStats player = playerStatsNetworkObject.GetComponent<PlayerStats>();
        restaurantBehaviourArray[groupNumber].AddPosition(player.transform, player.OwnerClientId);
    }
    [ClientRpc]
    private void ActivatePlayerInputClientRPC(NetworkObjectReference playerInputNetworkObjectReference)
    {
        playerInputNetworkObjectReference.TryGet(out NetworkObject playerInputNetworkObject);
        PlayerController playerController = playerInputNetworkObject.GetComponent<PlayerController>();
        playerController.enabled = true;
    }
    [ClientRpc]
    public void DesactivateGroupCanvasClientRPC()
    {
        this.gameObject.SetActive(false);
        restaurantMusic.Play();
    }
    [ClientRpc]
    private void SetCameraClientRPC(int groupID, ulong id)
    {
        if (id != NetworkManager.Singleton.LocalClientId) return;
        cameraSelector.ActivateCamera(groupID);
    }
}
