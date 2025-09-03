using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGenerator : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    private GridLevelGenerator levelGenerator;

    private ChooseGroupChicken chooseGroupChicken;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        levelGenerator = FindFirstObjectByType<GridLevelGenerator>();
        chooseGroupChicken = FindFirstObjectByType<ChooseGroupChicken>();
        chooseGroupChicken.OnPlayerReady += SpawnPlayerForClientRPC;

    }
    [Rpc(SendTo.Server)]
    private void SpawnPlayerForClientRPC(ulong clientId)
    {
        Vector2Int startCell = levelGenerator.start;

        // Calcular posicion en mundo segun la  grid
        Vector3 origin = levelGenerator.transform.position
                         - new Vector3((levelGenerator.width - 1) * 0.5f * levelGenerator.cellSize,
                                       0f,
                                       (levelGenerator.height - 1) * 0.5f * levelGenerator.cellSize);

        Vector3 spawnPosition = origin + new Vector3(startCell.x * levelGenerator.cellSize, 0.5f, startCell.y * levelGenerator.cellSize);

        // Instanciar player como NetworkObject
        Debug.Log($"Spawning player for client {clientId} at {spawnPosition}");
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true); // CUIDADO EL TRUE
        player.transform.SetParent(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform, false);
        DesactivateMovementClientRPC(player.GetComponent<NetworkObject>());
    }
    [Rpc(SendTo.Everyone)]
    private void DesactivateMovementClientRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerInputController playerController = playerNetworkObject.GetComponent<PlayerInputController>();

        playerController.enabled = false;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        chooseGroupChicken.OnPlayerReady -= SpawnPlayerForClientRPC;

    }
}
