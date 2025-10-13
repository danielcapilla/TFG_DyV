using System;
using System.Collections;
using System.Collections.Generic;
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
    [Header("Música")]
    [SerializeField] private AudioSource chickenMusic;
    [SerializeField] private AudioSource winMusic;
    [SerializeField] private AudioSource collisionSound;

    [Header("Equipos")]
    [SerializeField] private TeamMenager teamMenager;
    [Header("UI")]
    [SerializeField] private GameObject movementPanel;
    [SerializeField] private GameObject hostCanvas;

    [Header("Base de Datos")]
    [SerializeField] private DataBaseCommander dataBaseCommander;
    private bool dbSent = false;

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
        else
            hostCanvas.SetActive(false);
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
            ShowMovementPanelRPC(false);
            StartCoroutine(PlayWinMusicAndLoadScene());
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
                //inputController.OnObstaculeCollided += PlayCollisionSoundForGroup; Falta desuscribirse
                ActivatePlayerInputRPC(player);
            }
        }
        ShowMovementPanelRPC(true);
    }
    [Rpc(SendTo.NotMe)]
    private void ShowMovementPanelRPC(bool b)
    {
        if (movementPanel != null)
            movementPanel.SetActive(b);
    }

    [Rpc(SendTo.Everyone)]
    private void ActivatePlayerInputRPC(NetworkObjectReference playerInputNetworkObjectReference)
    {
        playerInputNetworkObjectReference.TryGet(out NetworkObject playerInputNetworkObject);
        PlayerInput playerInput = playerInputNetworkObject.GetComponentInChildren<PlayerInput>();
        playerInput.enabled = true;
        if (movementPanel != null)
            movementPanel.SetActive(true);
        ActivateChickenMusic();
    }
    [Rpc(SendTo.Everyone)]
    private void DesactivatePlayerInputRPC(NetworkObjectReference playerInputNetworkObjectReference)
    {
        playerInputNetworkObjectReference.TryGet(out NetworkObject playerInputNetworkObject);
        PlayerInput playerInput = playerInputNetworkObject.GetComponentInChildren<PlayerInput>();
        playerInput.enabled = false;
        ActivateChickenMusic();
    }
    private IEnumerator PlayWinMusicAndLoadScene()
    {
        ActivateWinMusicRPC();
        if (winMusic != null && winMusic.clip != null)
        {
            yield return new WaitForSeconds(winMusic.clip.length);
        }
        // Envio de datos a la base de datos
        dataBaseCommander.RegisterChickenGridCurrent(PlayerData.ClassCode, PlayerData.ClassCode, _ =>
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
        });
        //NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
    }
    [Rpc(SendTo.Everyone)]
    private void ActivateWinMusicRPC()
    {
        winMusic.Play();
    }
    private void ActivateChickenMusic()
    {
        chickenMusic.Play();
    }
    public void PlayCollisionSoundForGroup(int groupId)
    {
        TeamInfo teamInfo = teamMenager.teams[groupId];
        ulong[] targetClients = teamInfo.integrantes.ToArray();
        PlayCollisionSoundClientRPC(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClients
            }
        });
    }
    public void SaveMatchToDB()
    {
        if (dataBaseCommander == null)
            dataBaseCommander = GameObject.FindFirstObjectByType<DataBaseCommander>();

        // Usa los códigos que corresponda (ajusta si tienes studentClassCode)
        dataBaseCommander.RegisterChickenGridCurrent(PlayerData.ClassCode, PlayerData.ClassCode, _ =>
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
        });
    }
    [ClientRpc]
    private void PlayCollisionSoundClientRPC(ClientRpcParams clientRpcParams)
    {
        if (collisionSound != null)
            collisionSound.Play();
    }
}
