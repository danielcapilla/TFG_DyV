using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChooseGroup : NetworkBehaviour
{
    // Grupo
    PlayerStats player;
    [SerializeField] public TeamMenager teamManager;
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject groupCanvas;
    private Button previousButton;
    [SerializeField] private Button[] buttons;

    private Dictionary<ulong, bool> playerReadyDictionary;
    public static List<ulong> connectedPlayers;


    private bool host = true;

    // Eventos
    public delegate void PlayerReady(ulong id);
    public event PlayerReady OnPlayerReady;
    public delegate void OnGameStart();
    public event OnGameStart OnGameStartEvent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        readyButton.gameObject.SetActive(false);
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
        TeamInfo teamInfo = teamManager.teams[player.idGrupo.Value];
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
            DesactivateGroupCanvasRPC();
            OnGameStartEvent?.Invoke();
            
        }
    }

    [Rpc(SendTo.Everyone)]
    public void DesactivateGroupCanvasRPC()
    {
        groupCanvas.gameObject.SetActive(false);
    }

}
