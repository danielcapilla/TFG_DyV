using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.InputSystem;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

    private LateJoinsBehaviour lateJoinsBehaviour;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
            NetworkManager.Singleton.SceneManager.OnUnload += UnSceceLoaded;
            lateJoinsBehaviour = FindObjectOfType<LateJoinsBehaviour>();
        }

    }

    private void UnSceceLoaded(ulong clientId, string sceneName, AsyncOperation asyncOperation)
    {
        if (IsServer && sceneName == "MinijuegoRestaurante")
        {
            //lateJoinsBehaviour.aprovedConection = true;
            foreach (ulong id in NetworkManager.ConnectedClientsIds)
            {
                NetworkObject playerNetworkObject = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform.GetChild(0).GetComponent<NetworkObject>();
                playerNetworkObject.Despawn(true);
            }
        }
    }

    private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsServer && sceneName == "MinijuegoRestaurante")
        {
            lateJoinsBehaviour.aprovedConection = false;
            foreach (ulong id in clientsCompleted)
            {
               
                //Pongos los fighters como hijos del player
                //arrayPlayers[id].GetComponent<PlayerNetworkConfig>().InstantiateCharacterServerRpc(id);
                GameObject playerGameObject = Instantiate(playerPrefab);
                playerGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
                playerGameObject.transform.SetParent(NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform, false);
                DesactivateMovementClientRPC(playerGameObject.GetComponent<NetworkObject>());
                //NetworkManager.Singleton.ConnectedClients[id].PlayerObject;
                //PlayerNetworkConfig.Instance.InstantiateCharacterServerRpc(id);
                //player.transform.SetParent(transform, false);
            }
        }
    }
    [ClientRpc]
    private void DesactivateMovementClientRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerController playerController = playerNetworkObject.GetComponent<PlayerController>();

        playerController.enabled = false;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
        NetworkManager.Singleton.SceneManager.OnUnload -= UnSceceLoaded;

    }
}
