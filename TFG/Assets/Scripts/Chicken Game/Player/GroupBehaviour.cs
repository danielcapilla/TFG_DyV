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
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer) return;
        gameManager = FindFirstObjectByType<GameManagerChicken>();
        gameManager.OnPlayerSpawned += HandlePlayerSpawned;

    }

    private void HandlePlayerSpawned(NetworkObjectReference playerNOR, int idGroup)
    {
        Debug.Log("Handling player spawned for group: " + idGroup);
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        // Obtener todos los clientes del grupo especifico
        ulong[] targetClients = teamInfo.integrantes.ToArray();
        if(targetClients.Length == 0) return; // Si no hay clientes en el grupo, salir
        playerNOR.TryGet(out NetworkObject playerNetworkObject);
        PlayerInputController playerController = playerNetworkObject.GetComponentInChildren<PlayerInputController>();
        if (!playerController)
        {
            return;
        }
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
        Debug.Log("Player obtained for group: " + player.name);
    }
    private ulong[] GetClientsInGroup(int targetGroupId)
    {
        List<ulong> clientIds = new List<ulong>();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // Obtener el idGrupo de cada jugador como mencionaste
            PlayerStats playerStats = client.PlayerObject.GetComponent<PlayerStats>();
            if (playerStats != null && playerStats.idGrupo.Value == targetGroupId)
            {
                clientIds.Add(client.ClientId);
            }
        }

        return clientIds.ToArray();
    }
    public void AddCommand(ICommand command)
    {
        commandQueue.Enqueue(command);
    }

    public void ExecuteTurn()
    {
        while (commandQueue.Count > 0)
        {
            ICommand command = commandQueue.Dequeue();
            command.Execute(player);
        }
    }
}
