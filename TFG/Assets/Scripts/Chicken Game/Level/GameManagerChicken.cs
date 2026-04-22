using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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
    [SerializeField] private TeamManager teamMenager;
    [Header("UI")]
    [SerializeField] private GameObject movementPanel;
    [SerializeField] private GameObject movementPanelToggle;
    [SerializeField] private GameObject hostCanvas;
    [SerializeField] private GameObject groupPanel;

    [Header("Base de Datos")]
    [SerializeField] private DataBaseCommander dataBaseCommander;
    private string studentClassCode = "A";

    // Eventos
    public delegate void PlayerSpawned(NetworkObjectReference playerNOR, int idGroup);
    public event PlayerSpawned OnPlayerSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient && !IsHost)
        {
            ClassCodeRPC(PlayerData.ClassCode);
        }
        if (IsServer)
        {
            ChooseGroup.OnGameStartEvent += StartGame;
            groupBehaviour.OnExecutedTurn += CalculatePunctuation;
            GridLevelGenerator.Instance.OnLevelGenerated += ActivateGroupPanelRPC;

        }
        else
        {
            hostCanvas.SetActive(false);
        }
            

    }
    [Rpc(SendTo.NotMe)]
    private void ActivateGroupPanelRPC()
    {
        groupPanel.SetActive(true); 
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
            ChooseGroup.OnGameStartEvent -= StartGame;
            groupBehaviour.OnExecutedTurn -= CalculatePunctuation;
            GridLevelGenerator.Instance.OnLevelGenerated -= ActivateGroupPanelRPC;
        }
        else
        {
            
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
                inputController.OnObstaculeCollided += PlayCollisionSoundForGroup; //Falta desuscribirse
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
        if(movementPanelToggle != null)
            movementPanelToggle.SetActive(b);
    }

    [Rpc(SendTo.Everyone)]
    private void ActivatePlayerInputRPC(NetworkObjectReference playerInputNetworkObjectReference)
    {
        playerInputNetworkObjectReference.TryGet(out NetworkObject playerInputNetworkObject);
        PlayerInput playerInput = playerInputNetworkObject.GetComponentInChildren<PlayerInput>();
        playerInput.enabled = true;
        //if (movementPanel != null)
        //    movementPanel.SetActive(true);
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
        dataBaseCommander.RegisterChickenGridCurrent(PlayerData.ClassCode, studentClassCode, _ =>
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
    [Rpc(SendTo.Server)]
    private void ClassCodeRPC(FixedString64Bytes classCode)
    {
        // Solo interesa el ultimo que llegue
        studentClassCode = classCode.ToString();
    }
    [ClientRpc]
    private void PlayCollisionSoundClientRPC(ClientRpcParams clientRpcParams)
    {
        if (collisionSound != null)
            collisionSound.Play();
    }
}
