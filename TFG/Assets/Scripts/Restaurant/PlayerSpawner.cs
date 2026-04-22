using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.InputSystem;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameSceneBehaviour[] gameSceneBehaviourArray;
    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    private void Awake()
    {
        if (!IsOffline) return;
        // Offline: suscribirse a OnPlayerReady para spawnear el jugador local
        ChooseGroup.OnPlayerReady += SpawnOffline;
    }

    private void OnDestroy()
    {
        ChooseGroup.OnPlayerReady -= SpawnOffline;
    }

    private void SpawnOffline(ulong clientId)
    {
        ChooseGroup.OnPlayerReady -= SpawnOffline;
        if (playerPrefab == null) return;
        GameObject player = Instantiate(playerPrefab);
        if (gameSceneBehaviourArray != null && gameSceneBehaviourArray.Length > 0
            && gameSceneBehaviourArray[0].spawnPositions != null
            && gameSceneBehaviourArray[0].spawnPositions.Length > 0)
        {
            player.transform.position = gameSceneBehaviourArray[0].spawnPositions[0].position;
            player.transform.rotation = gameSceneBehaviourArray[0].spawnPositions[0].rotation;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            ChooseGroup.OnPlayerReady += SpawnPlayer;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoadedCallback;
            NetworkManager.Singleton.SceneManager.OnUnload += SceneUnloadedCallback;
        }

    }
    private void SpawnPlayer(ulong clientId)
    {
        GameObject player = Instantiate(playerPrefab);
        var netObj = player.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId);
        player.transform.SetParent(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform, false);


        PlayerStats playerStats = player.GetComponentInParent<PlayerStats>();
        int groupNumber = playerStats.idGrupo.Value;
        Debug.Log($"SpawnPlayer: grupo {groupNumber} para client {clientId}");

        if (groupNumber >= 0 && groupNumber < gameSceneBehaviourArray.Length)
        {
            SetPlayerPositionRPC(groupNumber, playerStats.NetworkObject);
        }
        else
        {
            Debug.LogError($"SpawnPlayer: grupo inválido ({groupNumber}) para client {clientId}");
        }

        DesactivateMovementClientRPC(netObj);
    }
    [Rpc(SendTo.Everyone)]
    private void RemovePlayerPositionRPC(NetworkObjectReference playerStatsNetworkObjectReference)
    {
        playerStatsNetworkObjectReference.TryGet(out NetworkObject playerStatsNetworkObject);
        PlayerStats player = playerStatsNetworkObject.GetComponent<PlayerStats>();
        if (player.idGrupo.Value != -1)
        {
            player.transform.SetParent(null, true); // RemovePosition: detach from slot
        }
    }
    [Rpc(SendTo.Everyone)]
    private void SetPlayerPositionRPC(int groupNumber, NetworkObjectReference playerStatsNetworkObjectReference)
    {
        playerStatsNetworkObjectReference.TryGet(out NetworkObject playerStatsNetworkObject);
        PlayerStats player = playerStatsNetworkObject.GetComponent<PlayerStats>();
        { var slot = gameSceneBehaviourArray[groupNumber].spawnPositions.Length > 0 ? gameSceneBehaviourArray[groupNumber].spawnPositions[0] : gameSceneBehaviourArray[groupNumber].transform; player.transform.GetChild(0).SetPositionAndRotation(slot.position, slot.rotation); }
    }
    private void SceneUnloadedCallback(ulong clientId, string sceneName, AsyncOperation asyncOperation)
    {
        if (IsServer)
        {
            //lateJoinsBehaviour.aprovedConection = true;
            foreach (ulong id in NetworkManager.ConnectedClientsIds)
            {
                if (id != OwnerClientId)
                {
                    NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform.GetChild(0).GetComponent<NetworkObject>();
                    playerNetworkObject.Despawn(true);
                }

            }
        }
    }

    private void SceneLoadedCallback(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {

        LateJoinsBehaviour.aprovedConection = false;           

    }
    [Rpc(SendTo.Everyone)]
    private void DesactivateMovementClientRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerController playerController = playerNetworkObject.GetComponent<PlayerController>();

        playerController.enabled = false;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if(IsServer)
        {
            ChooseGroup.OnPlayerReady -= SpawnPlayer;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoadedCallback;
            NetworkManager.Singleton.SceneManager.OnUnload -= SceneUnloadedCallback;
        }


    }
}
