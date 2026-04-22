using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChooseGroup : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject groupCanvas;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private GameObject buttonPrefab;

    public static event System.Action<ulong> OnPlayerReady;
    public static event System.Action OnGameStartEvent;

    public static List<ulong> connectedPlayers;

    private Dictionary<ulong, bool> playerReadyDictionary;
    private int selectedGroup = -1;
    private bool gameStarted = false; // evita doble disparo de OnPlayerReady
    private List<Button> groupButtons = new List<Button>();

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;
    private TeamMenager teamManager => TeamMenager.Instance;

    // ── Offline ───────────────────────────────────────────────────────────────

    private void Start()
    {
        if (!IsOffline) return;

        connectedPlayers = new List<ulong> { 0 };
        if (groupCanvas != null) groupCanvas.SetActive(false);

        PlayerStats localPlayer = FindFirstObjectByType<PlayerStats>();
        if (localPlayer != null) localPlayer.idGrupo.Value = 0;

        if (teamManager != null && teamManager.teams.Count > 0)
            teamManager.teams[0].integrantes.Add(0);

        // En offline disparar OnPlayerReady igual que en online
        // para que PlayerSpawner reciba el evento y spawnee al jugador
        OnPlayerReady?.Invoke(0);
        OnGameStartEvent?.Invoke();
    }

    // ── Online ────────────────────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // Esperar a que TeamMenager.Instance este disponible antes de usarlo
            StartCoroutine(InitServerDelayed());
        }
        else
        {
            // El cliente espera recibir el RPC con el numero de equipos
            // No hace nada aqui — los botones se crean al recibir SendTeamConfigClientRpc
        }
    }

    private IEnumerator InitServerDelayed()
    {
        // Esperar hasta que TeamMenager.Instance este listo
        yield return new WaitUntil(() => TeamMenager.Instance != null);

        playerReadyDictionary = new Dictionary<ulong, bool>();
        gameStarted = false;
        // El host es observador — excluirlo de los jugadores
        connectedPlayers = NetworkManager.Singleton.ConnectedClientsIds
            .Where(id => id != OwnerClientId).ToList();

        // El host no juega: ocultar canvas
        if (groupCanvas != null) groupCanvas.SetActive(false);

        // Enviar totalTeams a los clientes para que generen los botones
        SendTeamConfigClientRpc(teamManager.totalTeams);
    }

    [ClientRpc]
    private void SendTeamConfigClientRpc(int totalTeams)
    {
        // Solo ejecutar en clientes puros (no en el servidor/host)
        if (IsServer) return;

        SetupGroupButtons(totalTeams);
        readyButton.gameObject.SetActive(false);
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(ReadyPlayer);
    }

    private void SetupGroupButtons(int totalTeams)
    {
        if (buttonsContainer == null || buttonPrefab == null) return;

        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);
        groupButtons.Clear();

        for (int i = 0; i < totalTeams; i++)
        {
            int groupIndex = i;
            GameObject btn = Instantiate(buttonPrefab, buttonsContainer);

            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = $"{i + 1}";

            Button button = btn.GetComponent<Button>();
            button.onClick.AddListener(() => SelectGroup(groupIndex, button));
            groupButtons.Add(button);
        }
    }

    private void SelectGroup(int groupIndex, Button clicked)
    {
        foreach (var btn in groupButtons) btn.interactable = true;
        clicked.interactable = false;
        selectedGroup = groupIndex;
        ChangeGroupRpc(NetworkManager.Singleton.LocalClientId, groupIndex);
        readyButton.gameObject.SetActive(true);
    }

    [Rpc(SendTo.Server)]
    private void ChangeGroupRpc(ulong id, int groupNumber)
    {
        PlayerStats ps = NetworkManager.Singleton.ConnectedClients[id].PlayerObject
            .GetComponent<PlayerStats>();
        ps.idGrupo.Value = groupNumber;
    }

    private void ReadyPlayer()
    {
        if (selectedGroup < 0) return;
        readyButton.interactable = false;
        foreach (var btn in groupButtons) btn.interactable = false;
        ReadyPlayerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void ReadyPlayerRpc(ulong id)
    {
        PlayerStats ps = NetworkManager.Singleton.ConnectedClients[id].PlayerObject
            .GetComponent<PlayerStats>();
        int group = ps.idGrupo.Value;

        if (group < 0 || group >= teamManager.teams.Count)
        {
            ShowWarningClientRpc("Selecciona un equipo primero.",
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { id } } });
            ReenableUIClientRpc(
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { id } } });
            return;
        }

        TeamInfo teamInfo = teamManager.teams[group];
        if (teamInfo.integrantes.Count >= teamManager.maxPlayersPerTeam)
        {
            ShowWarningClientRpc($"Equipo lleno (max {teamManager.maxPlayersPerTeam}).",
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { id } } });
            ReenableUIClientRpc(
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { id } } });
            return;
        }

        // Guard: evitar que el mismo jugador se procese dos veces
        if (playerReadyDictionary.ContainsKey(id) && playerReadyDictionary[id]) return;

        teamInfo.integrantes.Add(id);
        playerReadyDictionary[id] = true;
        CheckAllReady();
    }

    private void CheckAllReady()
    {
        bool allReady = connectedPlayers.All(id =>
            playerReadyDictionary.ContainsKey(id) && playerReadyDictionary[id]);

        if (!allReady) return;

        if (!AreTeamsValid())
        {
            playerReadyDictionary.Clear();
            foreach (var team in teamManager.teams) team.integrantes.Clear();
            ShowWarningClientRpc(
                $"Cada equipo debe tener entre {teamManager.minPlayersPerTeam} y {teamManager.maxPlayersPerTeam} jugadores.");
            ReenableUIClientRpc();
        }
        else
        {
            if (gameStarted) return; // guard absoluto contra doble disparo
            gameStarted = true;

            var toSpawn = new System.Collections.Generic.List<ulong>(connectedPlayers);
            playerReadyDictionary.Clear();

            foreach (ulong clientId in toSpawn)
                OnPlayerReady?.Invoke(clientId);
            HideCanvasRpc();
            OnGameStartEvent?.Invoke();
        }
    }

    private bool AreTeamsValid()
    {
        foreach (var team in teamManager.teams)
        {
            int count = team.integrantes.Count;
            if (count == 0) continue;
            if (count < teamManager.minPlayersPerTeam || count > teamManager.maxPlayersPerTeam)
                return false;
        }
        return true;
    }

    [ClientRpc]
    private void ShowWarningClientRpc(string msg, ClientRpcParams p = default) =>
        NotificationManager.Instance.ShowWarningNotification(msg);

    [ClientRpc]
    private void ReenableUIClientRpc(ClientRpcParams p = default)
    {
        readyButton.interactable = true;
        foreach (var btn in groupButtons) btn.interactable = true;
    }

    [Rpc(SendTo.Everyone)]
    public void HideCanvasRpc() => groupCanvas.SetActive(false);
}
