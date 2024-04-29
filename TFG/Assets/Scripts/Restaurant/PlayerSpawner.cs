using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    private bool gameStarted = false;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
        }

    }

    private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsServer && sceneName == "MinijuegoRestaurante" && !gameStarted)
        {
            gameStarted = true;
            foreach (ulong id in clientsCompleted)
            {
               
                //Pongos los fighters como hijos del player
                //arrayPlayers[id].GetComponent<PlayerNetworkConfig>().InstantiateCharacterServerRpc(id);
                GameObject playerGameObject = Instantiate(playerPrefab);
                playerGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
                playerGameObject.transform.SetParent(NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform, false);
                //NetworkManager.Singleton.ConnectedClients[id].PlayerObject;
                //PlayerNetworkConfig.Instance.InstantiateCharacterServerRpc(id);
                //player.transform.SetParent(transform, false);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
    }
}
