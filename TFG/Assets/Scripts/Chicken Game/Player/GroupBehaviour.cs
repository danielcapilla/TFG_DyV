using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GroupBehaviour : NetworkBehaviour
{
    private Queue<ICommand> commandQueue = new Queue<ICommand>();
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
        if(targetClients.Length == 0) return; // Si no hay clientes en el grupo, salir
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
    public void AddCommand(ICommand command)
    {
        commandQueue.Enqueue(command);
        OnCommandAdded?.Invoke(NetworkManager.Singleton.LocalClientId);
    }

    public void ExecuteTurn()
    {
        while (commandQueue.Count > 0)
        {
            ICommand command = commandQueue.Dequeue();
            command.Execute(player);
            OnExecutedTurn?.Invoke(NetworkManager.Singleton.LocalClientId);
        }
    }
}
