using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class ChooseGroupChicken : NetworkBehaviour
{

    [Header("Referencias")]
    [SerializeField] public TeamMenager teamManager;
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject groupCanvas;
    //[SerializeField] private Countdown countdown;

    [SerializeField] private Button[] buttons;
    private Button previousButton;
    private PlayerStats player;
    private Dictionary<ulong, bool> playerReadyDictionary;
    public static List<ulong> connectedPlayers;

    // Debug para el host
    private bool host = true;

    public delegate void PlayerReady(ulong id);
    public event PlayerReady OnPlayerReady;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        readyButton.gameObject.SetActive(false);
        //buttons = GetComponentsInChildren<Button>();

        if (IsServer)
        {

            playerReadyDictionary = new Dictionary<ulong, bool>();
            connectedPlayers = NetworkManager.Singleton.ConnectedClientsIds.ToList<ulong>();
            Debug.Log("Connected Players: " + connectedPlayers.Count);
            if (host) return;
            groupCanvas.SetActive(false);
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
        // CAMBIO GORDO AHORA PLAYER STATS ESTA EN USER
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponent<PlayerStats>();
        //SetPlayerPositionPart1ClientRPC(player.NetworkObject);
        player.idGrupo.Value = groupNumber;
        //SetPlayerPositionPart2ClientRPC(groupNumber, player.NetworkObject);

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
        ////////////////////////////////////////////////////////////////////
        Debug.Log("Player " + id + " joined group " + player.idGrupo.Value);
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamManager.teams[player.idGrupo.Value];
        /////////////////////////////////////////////////////////////////////
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
            //countdown.CambiarVariable();
            foreach (ulong playerId in connectedPlayers)
            {
                //cameraSelector.ActivateCamera(NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerStats>().idGrupo.Value);
                ///////////////////////////////////////////////////////////////////////////////////////////
                //SetCameraClientRPC(NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerStats>().idGrupo.Value, playerId);
                //////////////////////////////////////////////////////////////////////////////////////////
                //PlayerInput playerInput = NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerInput>();
                //ActivatePlayerInputRPC(playerInput.GetComponent<NetworkObject>());
            }
        }
    }
    [Rpc(SendTo.Everyone)]
    private void ActivatePlayerInputRPC(NetworkObjectReference playerInputNetworkObjectReference)
    {
        playerInputNetworkObjectReference.TryGet(out NetworkObject playerInputNetworkObject);
        PlayerInputController playerController = playerInputNetworkObject.GetComponent<PlayerInputController>();
        playerController.enabled = true;
    }
    [Rpc(SendTo.Everyone)]
    public void DesactivateGroupCanvasRPC()
    {
        groupCanvas.SetActive(false);
        ///////////////////////////////////////////////////////////////////////
        //restaurantMusic.Play();
        ///////////////////////////////////////////////////////////////////////
    }
}
