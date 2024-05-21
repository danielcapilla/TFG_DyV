using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.InputSystem;
using System.Linq;

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
    private RestaurantBehaviour[] restaurantBehaviourArray;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        readyButton.gameObject.SetActive(false);
        if(IsServer)
        {
            this.gameObject.SetActive(false);
            playerReadyDictionary = new Dictionary<ulong, bool>();
            connectedPlayers = NetworkManager.Singleton.ConnectedClientsIds.ToList<ulong>();
            connectedPlayers.Remove(OwnerClientId);
            restaurantBehaviourArray = FindObjectsOfType<RestaurantBehaviour>();
        }
        
    }
    public void Cambio()
    {
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        CambioServerRPC(NetworkManager.Singleton.LocalClientId, (int.Parse(clickedButton.GetComponentInChildren<TextMeshProUGUI>().text))-1);
        readyButton.gameObject.SetActive(true);
    }
    [ServerRpc (RequireOwnership = false)]
    private void CambioServerRPC(ulong id, int groupNumber)
    {
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponentInChildren<PlayerStats>();
        if(player.idGrupo.Value != -1)
        {
            restaurantBehaviourArray[player.idGrupo.Value].RemovePosition(player.transform);
        }
        player.idGrupo.Value = groupNumber;
        restaurantBehaviourArray[groupNumber].AddPosition(player.transform);
    }
    public void ReadyPlayer()
    {
        ReadyPlayerServerRPC(NetworkManager.Singleton.LocalClientId);
        
    }
    [ServerRpc (RequireOwnership = false)]
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
                cameraSelector.ActivateCamera(NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerStats>().idGrupo.Value);
                PlayerInput playerInput = NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerInput>();
                ActivatePlayerInputClientRPC(playerInput.GetComponent<NetworkObject>());
            }
        }
    }
    [ClientRpc]
    private void SetPlayerPositionClientRPC()
    {

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
    }
}
