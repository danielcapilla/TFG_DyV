using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Localization.Platform.Android;
using UnityEngine;

public class GroupBehaviour : NetworkBehaviour
{
    private Queue<ICommand> commandQueue = new Queue<ICommand>();
    private PlayerInputController player;

    private PlayerGenerator playerGenerator;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerGenerator = FindFirstObjectByType<PlayerGenerator>();
        playerGenerator.OnPlayerSpawned += ObtainPlayerForGroup;

    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        playerGenerator.OnPlayerSpawned -= ObtainPlayerForGroup;
    }
    private void ObtainPlayerForGroup(GameObject player)
    {
        this.player = player.GetComponent<PlayerInputController>();
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
