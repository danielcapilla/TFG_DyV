using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GroupBehaviour : NetworkBehaviour
{
    private PlayerInputController player;
    [Header("Referencias")]
    [SerializeField] private TeamMenager teamMenager;
    [SerializeField] private GameManagerChicken gameManager;
    [SerializeField] private ChooseGroup chooseGroup;

    // Eventos
    public event Action<ulong, CommandType> OnCommandAdded;
    public event Action<PlayerInputController, int> OnExecutedTurn;
    public event Action<int> OnExecuteTurn;

    // Estado por grupo
    private readonly Dictionary<int, bool> groupIsExecuting = new Dictionary<int, bool>();
    private readonly Dictionary<int, Coroutine> groupCoroutines = new Dictionary<int, Coroutine>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;

        gameManager.OnPlayerSpawned += HandlePlayerSpawned;

        chooseGroup.OnGameStartEvent += () =>
        {
            for (int i = 0; i < teamMenager.teams.Count; i++)
            {
                groupIsExecuting[i] = false;
                if (groupCoroutines.ContainsKey(i))
                    groupCoroutines[i] = null;
                else
                    groupCoroutines.Add(i, null);
            }
        };
    }

    private void HandlePlayerSpawned(NetworkObjectReference playerNOR, int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        ulong[] targetClients = teamInfo.integrantes.ToArray();

        if (!playerNOR.TryGet(out NetworkObject playerNetworkObject))
            return;

        PlayerInputController playerController = playerNetworkObject.GetComponentInChildren<PlayerInputController>();
        teamInfo.playerPrefab = playerController;

        if (targetClients.Length == 0) return;

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
        if (gameManager != null)
            gameManager.OnPlayerSpawned -= HandlePlayerSpawned;
    }

    [ClientRpc]
    private void ObtainPlayerForGroupClientRPC(NetworkObjectReference playerNetworkObjectReference,
        ClientRpcParams clientRpcParams = default)
    {
        if (playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            PlayerInputController playerController = playerNetworkObject.GetComponentInChildren<PlayerInputController>();
            this.player = playerController;
        }
    }

    public void AddCommand(CommandType commandType)
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        AddCommandRPC(clientId, commandType);
        OnCommandAdded?.Invoke(clientId, commandType);
    }

    [Rpc(SendTo.Server)]
    private void AddCommandRPC(ulong clientId, CommandType commandType)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        int groupId = client.PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[groupId];

        ICommand command = CreateCommandFromType(commandType);
        teamInfo.commandQueue.Enqueue(command);

        //Debug.Log($"[GroupBehaviour] Command {commandType} added for group {groupId} by player {clientId}");
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
        if (!IsServer) return;

        if (!groupIsExecuting.ContainsKey(groupId))
            groupIsExecuting[groupId] = false;

        if (groupIsExecuting[groupId])
        {
            //Debug.Log($"[GroupBehaviour] Turn already executing for group {groupId}, ignored.");
            return;
        }

        if (groupCoroutines[groupId] != null)
        {
            StopCoroutine(groupCoroutines[groupId]);
            groupCoroutines[groupId] = null;
        }

        groupCoroutines[groupId] = StartCoroutine(ExecuteTurnCoroutine(groupId));
    }

    private IEnumerator ExecuteTurnCoroutine(int groupId)
    {
        groupIsExecuting[groupId] = true;

        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[groupId];
        PlayerInputController playerController = teamInfo.playerPrefab;

        if (playerController == null)
        {
            groupIsExecuting[groupId] = false;
            yield break;
        }

        // Snapshot de comandos para este turno (proteccion)
        var turnCommands = new List<ICommand>(teamInfo.commandQueue);
        teamInfo.commandQueue.Clear();

        OnExecuteTurn?.Invoke(groupId);
        //Debug.Log($"[GroupBehaviour] Executing {turnCommands.Count} commands for group {groupId}");

        foreach (var command in turnCommands)
        {
            command.Execute(playerController);

            // Esperar a que el movimiento arranque (si produce movimiento)
            yield return new WaitForSeconds(0f); // asegura un frame
            if (playerController.IsMoving)
            {
                // Esperar a que termine
                while (playerController.IsMoving)
                    yield return null;
            }
            if (playerController.LastMoveBlocked)
            {
                //Debug.Log($"[GroupBehaviour] Movimiento bloqueado. Cancelando resto de comandos del turno del grupo {groupId}");
                break; // turnCommands restante se descarta
            }

            // Pequeño margen de red
            yield return new WaitForSeconds(0.05f);
        }

        OnExecutedTurn?.Invoke(playerController, groupId);
        //Debug.Log($"[GroupBehaviour] Finished executing turn for group {groupId}");

        groupIsExecuting[groupId] = false;
        groupCoroutines[groupId] = null;
    }
}