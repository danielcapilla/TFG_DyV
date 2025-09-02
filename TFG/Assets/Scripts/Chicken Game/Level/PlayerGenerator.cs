using UnityEngine;
using Unity.Netcode;

public class PlayerGenerator : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private GridLevelGenerator levelGenerator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Solo el servidor spawnea
        if (!IsServer) return;

        levelGenerator = FindFirstObjectByType<GridLevelGenerator>();

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId) continue; 
            SpawnPlayerForClient(clientId);
        }

        // Escuchar nuevas conexiones (para late join)
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayerForClient;
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        Vector2Int startCell = levelGenerator.start;

        // Calcular posición en mundo según tu grid
        Vector3 origin = levelGenerator.transform.position
                         - new Vector3((levelGenerator.width - 1) * 0.5f * levelGenerator.cellSize,
                                       0f,
                                       (levelGenerator.height - 1) * 0.5f * levelGenerator.cellSize);

        Vector3 spawnPosition = origin + new Vector3(startCell.x * levelGenerator.cellSize, 0.5f, startCell.y * levelGenerator.cellSize);

        // Instanciar player como NetworkObject
        Debug.Log($"Spawning player for client {clientId} at {spawnPosition}");
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayerForClient;
    }
}
