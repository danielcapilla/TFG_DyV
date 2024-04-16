using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserNetworkConfig : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
    }
    [ServerRpc(RequireOwnership = false)]
    public void InstantiatePlayerServerRpc(ulong id)
    {
        Debug.Log("TT");
        GameObject playerGameObject = Instantiate(playerPrefab);
        playerGameObject.GetComponent<NetworkObject>().SpawnWithOwnership(id);
        playerGameObject.transform.SetParent(transform, false);
    }

}
