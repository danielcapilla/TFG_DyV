using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class GroupBehaviour : NetworkBehaviour
{
    //private Queue<ICommand> commandQueue = new Queue<ICommand>();
    private PlayerInputController player;
    [SerializeField] TeamMenager teamMenager;
    private GameManagerChicken gameManager;
    // Eventos
    public event Action<ulong> OnCommandAdded;
    public event Action<ulong> OnExecutedTurn;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer) return;
        gameManager = FindFirstObjectByType<GameManagerChicken>();
        gameManager.OnPlayerSpawned += HandlePlayerSpawned;

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

        player = playerController;
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
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[groupId];
        while (teamInfo.commandQueue.Count > 0)
        {
           Debug.Log($"Executing command for player {groupId}");
            ICommand command = teamInfo.commandQueue.Dequeue();
            command.Execute(teamInfo.playerPrefab.GetComponent<PlayerInputController>());
            OnExecutedTurn?.Invoke(NetworkManager.Singleton.LocalClientId);
        }
    }
}
