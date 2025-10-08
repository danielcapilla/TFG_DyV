using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManagerChicken : NetworkBehaviour
{
    [Header("Evento")]
    [SerializeField] private ChooseGroup chooseGroup;
    [SerializeField] private GroupBehaviour groupBehaviour;
    //[Header("Tiempo")]
    //[SerializeField] private Countdown countdown;
    //[Header("Cámara")]
    //[SerializeField] private CameraSelector cameraSelector;
    //[Header("Música")]
    //[SerializeField] private AudioSource restaurantMusic;
    [Header("Equipos")]
    [SerializeField] private TeamMenager teamMenager;
    [Header("UI")]
    [SerializeField] private GameObject movementPanel;

    // Eventos
    public delegate void PlayerSpawned(NetworkObjectReference playerNOR, int idGroup);
    public event PlayerSpawned OnPlayerSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            chooseGroup.OnGameStartEvent += StartGame;
            groupBehaviour.OnExecutedTurn += CalculatePunctuation;
        }
    }

    private void CalculatePunctuation(PlayerInputController playerInput, int groupId)
    {
        Vector3 playerWorldPos = playerInput.targetPosition.Value;
        Vector2Int playerGridPos = GridLevelGenerator.Instance.WorldToGrid(playerWorldPos);

        int dist = GridLevelGenerator.Instance.GetDistanceToGoal(playerGridPos);
        float progress = GridLevelGenerator.Instance.GetProgress(playerGridPos);
        teamMenager.teams[groupId].Puntuacion = (int)(progress*100f);
        if (teamMenager.teams[groupId].Puntuacion == 100)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            chooseGroup.OnGameStartEvent -= StartGame;
            groupBehaviour.OnExecutedTurn -= CalculatePunctuation;
        }
    }
    private void StartGame()
    {
        foreach (ulong playerId in ChooseGroup.connectedPlayers)
        {
            NetworkObject player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
            PlayerInputController inputController = player.GetComponentInChildren<PlayerInputController>();
            if (inputController != null)
            {
                OnPlayerSpawned?.Invoke(player, player.GetComponent<PlayerStats>().idGrupo.Value);
                ActivatePlayerInputRPC(player);
            }
        }
        ShowMovementPanelRPC();
    }
    [Rpc(SendTo.NotMe)]
    private void ShowMovementPanelRPC()
    {
        if (movementPanel != null)
            movementPanel.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    private void ActivatePlayerInputRPC(NetworkObjectReference playerInputNetworkObjectReference)
    {
        playerInputNetworkObjectReference.TryGet(out NetworkObject playerInputNetworkObject);
        PlayerInput playerInput = playerInputNetworkObject.GetComponentInChildren<PlayerInput>();
        playerInput.enabled = true;
    }
}
