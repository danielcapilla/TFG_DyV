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
    [SerializeField] public TeamMenager teamManager;
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject groupCanvas;
    private Button previousButton;
    [SerializeField] private Button[] buttons;

    private Dictionary<ulong, bool> playerReadyDictionary;
    public static List<ulong> connectedPlayers;

    private bool host = false;

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    public delegate void PlayerReady(ulong id);
    public event PlayerReady OnPlayerReady;
    public delegate void OnGameStart();
    public event OnGameStart OnGameStartEvent;

    private void Start()
    {
        if (!IsOffline) return;

        // En offline: asignamos el jugador al equipo 0 automaticamente y arrancamos
        connectedPlayers = new List<ulong> { 0 };

        // Ocultamos el canvas de seleccion — no hace falta en offline
        if (groupCanvas != null) groupCanvas.SetActive(false);

        // Asignar equipo 0 al jugador local
        PlayerStats localPlayer = FindFirstObjectByType<PlayerStats>();
        if (localPlayer != null)
            localPlayer.idGrupo.Value = 0;  // NetworkVariable, funciona offline desde el mismo objeto

        // Añadir al equipo
        if (teamManager != null && teamManager.teams.Count > 0)
            teamManager.teams[0].integrantes.Add(0);

        // Arrancar el juego directamente
        OnGameStartEvent?.Invoke();
    }

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
            previousButton.interactable = true;

        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        clickedButton.interactable = false;
        previousButton = clickedButton;
        ChangeGroupRPC(NetworkManager.Singleton.LocalClientId,
            (int.Parse(clickedButton.GetComponentInChildren<TextMeshProUGUI>().text)) - 1);
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
            button.interactable = false;
        ReadyPlayerRPC(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    public void ReadyPlayerRPC(ulong id)
    {
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponent<PlayerStats>();
        TeamInfo teamInfo = teamManager.teams[player.idGrupo.Value];
        if (teamInfo.integrantes.Count >= teamManager.maxplayersPerTeam)
        {
            ShowWarningClientRPC($"Este grupo está lleno (máximo {teamManager.maxplayersPerTeam} jugadores).",
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { id } } });
            ReenableReadyUIClientRPC(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { id } } });
        }
        else
        {
            teamInfo.integrantes.Add(id);
            SetPlayerReady(id);
        }
    }

    public void SetPlayerReady(ulong id)
    {
        playerReadyDictionary[id] = true;
        bool allClientsReady = connectedPlayers.All(cid =>
            playerReadyDictionary.ContainsKey(cid) && playerReadyDictionary[cid]);

        if (allClientsReady)
        {
            if (!AreTeamsValid())
            {
                playerReadyDictionary.Clear();
                foreach (var team in teamManager.teams)
                    team.integrantes.Clear();
                ShowWarningClientRPC($"Cada grupo debe tener entre {teamManager.minPlayersPerTeam} y {teamManager.maxplayersPerTeam} jugadores.");
                ReenableReadyUIClientRPC();
            }
            else
            {
                foreach (ulong clientId in connectedPlayers)
                    OnPlayerReady?.Invoke(clientId);
                DesactivateGroupCanvasRPC();
                OnGameStartEvent?.Invoke();
            }
        }
    }

    [ClientRpc]
    private void ShowWarningClientRPC(string message, ClientRpcParams clientRpcParams = default)
    {
        NotificationManager.Instance.ShowWarningNotification(message);
    }

    [ClientRpc]
    private void ReenableReadyUIClientRPC(ClientRpcParams clientRpcParams = default)
    {
        if (readyButton != null) readyButton.interactable = true;
        if (buttons != null)
            foreach (Button button in buttons)
                if (button != null) button.interactable = true;
    }

    private bool AreTeamsValid()
    {
        foreach (var team in teamManager.teams)
        {
            int count = team.integrantes.Count;
            if (count == 0) continue;
            if (count < teamManager.minPlayersPerTeam || count > teamManager.maxplayersPerTeam)
                return false;
        }
        return true;
    }

    [Rpc(SendTo.Everyone)]
    public void DesactivateGroupCanvasRPC()
    {
        groupCanvas.gameObject.SetActive(false);
    }
}
