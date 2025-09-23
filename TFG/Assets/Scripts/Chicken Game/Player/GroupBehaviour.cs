using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class GroupBehaviour : NetworkBehaviour
{
    private PlayerInputController player;
    [SerializeField] TeamMenager teamMenager;
    private GameManagerChicken gameManager;
    private ChooseGroup chooseGroup;
    // Eventos
    public event Action<ulong> OnCommandAdded;
    public event Action<PlayerInputController,int> OnExecutedTurn;

    private Dictionary<int, bool> groupIsExecuting = new Dictionary<int, bool>();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;

        gameManager = FindFirstObjectByType<GameManagerChicken>();
        gameManager.OnPlayerSpawned += HandlePlayerSpawned;
        chooseGroup = FindFirstObjectByType<ChooseGroup>();
        chooseGroup.OnGameStartEvent += () =>
        {
            // Inicializar estado para todos los grupos al iniciar el juego
            for (int i = 0; i < teamMenager.teams.Count; i++)
            {
                groupIsExecuting[i] = false;
            }
        };
    }

    private void HandlePlayerSpawned(NetworkObjectReference playerNOR, int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        // Obtener todos los clientes del grupo especifico
        ulong[] targetClients = teamInfo.integrantes.ToArray();
        playerNOR.TryGet(out NetworkObject playerNetworkObject);
        PlayerInputController playerController = playerNetworkObject.GetComponentInChildren<PlayerInputController>();
        teamInfo.playerPrefab = playerController;
        if (targetClients.Length == 0) return; // Si no hay clientes en el grupo, salir
        ObtainPlayerForGroupClientRPC(playerNOR, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClients
            }
        });
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        gameManager.OnPlayerSpawned -= HandlePlayerSpawned;
    }
    [ClientRpc]
    private void ObtainPlayerForGroupClientRPC(NetworkObjectReference playerNetworkObjectReference,
        ClientRpcParams clientRpcParams = default)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerInputController playerController = playerNetworkObject.GetComponentInChildren<PlayerInputController>();

        this.player = playerController;
    }

    public void AddCommand(CommandType commandType)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        AddCommandRPC(clientId, commandType);
        OnCommandAdded?.Invoke(clientId);
    }

    [Rpc(SendTo.Server)]
    private void AddCommandRPC(ulong clientId, CommandType commandType)
    {

        int groupId = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[groupId];
        // Los enum si se pueden pasar por RPC
        // Crear el comando basado en el tipo recibido
        ICommand command = CreateCommandFromType(commandType);
        teamInfo.commandQueue.Enqueue(command);

        Debug.Log($"Command {commandType} added for group {groupId} by player {clientId}");
        
    }

    private ICommand CreateCommandFromType(CommandType type)
    {
        switch (type)
        {
            case CommandType.MoveLeft: return new MoveLeftCommand();
            case CommandType.MoveRight: return new MoveRightCommand();
            case CommandType.MoveUp: return new MoveUpCommand();
            case CommandType.MoveDown: return new MoveDownCommand();
            case CommandType.Wait: return new WaitCommand();
            default: return new WaitCommand();
        }
    }

    public void ExecuteTurn(int groupId)
    {
        if (!IsServer || groupIsExecuting[groupId]) return;

        StartCoroutine(ExecuteTurnCoroutine(groupId));
    }

    private IEnumerator ExecuteTurnCoroutine(int groupId)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[groupId];
        PlayerInputController playerController = teamInfo.playerPrefab;

        if (playerController == null)
        {
            Debug.LogError($"No player controller found for group {groupId}");
            yield break;
        }
        OnExecutedTurn?.Invoke(playerController, groupId);
        Debug.Log($"Executing {teamInfo.commandQueue.Count} commands for group {groupId}");

        while (teamInfo.commandQueue.Count > 0)
        {
            ICommand command = teamInfo.commandQueue.Dequeue();
            command.Execute(playerController);

            // Esperar a que el movimiento termine antes de continuar
            yield return WaitForMovementToComplete(playerController);

            // Pequeña pausa adicional para asegurar sincronizacion de red
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log($"Finished executing commands for group {groupId}");
        
    }

    private IEnumerator WaitForMovementToComplete(PlayerInputController playerController)
    {
        // Esperar hasta que el jugador deje de moverse
        while (playerController.IsMoving)
        {
            yield return null;
        }

        // Esperar un frame adicional para asegurar sincronización
        yield return null;
    }
}
