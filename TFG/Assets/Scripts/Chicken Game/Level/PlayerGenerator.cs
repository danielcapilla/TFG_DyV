using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGenerator : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    private GridLevelGenerator levelGenerator;

    private ChooseGroup chooseGroup;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        levelGenerator = FindFirstObjectByType<GridLevelGenerator>();
        chooseGroup = FindFirstObjectByType<ChooseGroup>();
        chooseGroup.OnPlayerReady += SpawnPlayerForClientRPC;
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnUnload += SceneUnloadedCallback;

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

    [Rpc(SendTo.Server)]
    private void SpawnPlayerForClientRPC(ulong clientId)
    {
        int idGrupo = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        TeamInfoChicken teamInfo = (TeamInfoChicken)chooseGroup.teamManager.teams[idGrupo];

        // Si el player ya fue spawneado, no hacer nada (solo 1 por cada grupo)
        if (teamInfo.spawnedPlayer) return;
        Vector2Int startCell = levelGenerator.start;

        // Calcular posicion en mundo segun la  grid
        Vector3 origin = levelGenerator.transform.position - new Vector3((levelGenerator.width - 1) * 0.5f * levelGenerator.cellSize,0f,
            (levelGenerator.height - 1) * 0.5f * levelGenerator.cellSize);

        Vector3 spawnXZ = origin + new Vector3(startCell.x * levelGenerator.cellSize, 10f, startCell.y * levelGenerator.cellSize); // Y=10 para raycast desde arriba

        // Raycast hacia abajo para encontrar el suelo
        RaycastHit hit;
        float spawnY = 0.5f; // valor por defecto si no hay suelo
        if (Physics.Raycast(spawnXZ, Vector3.down, out hit, 20f, LayerMask.GetMask("Default", "Ground")))
        {
            spawnY = hit.point.y + 0.1f; // 0.1f para evitar quedarse dentro del suelo
        }
        Vector3 spawnPosition = new Vector3(spawnXZ.x, spawnY, spawnXZ.z);

        // Instanciar player como NetworkObject
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true); // CUIDADO EL TRUE
        player.transform.SetParent(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform, true);
        //DesactivateMovementClientRPC(player.GetComponent<NetworkObject>());

        // Marcar que el player ya fue spawneado
        teamInfo.spawnedPlayer = true;
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
        chooseGroup.OnPlayerReady -= SpawnPlayerForClientRPC;
        if(!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnUnload -= SceneUnloadedCallback;

    }
}
