using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LateJoinsBehaviour :NetworkBehaviour
{
    
    public static bool aprovedConection = true;

    public static GameObject Instance { get; set; }
    private void Start()
    {

        if (!IsServer) return;
        //DontDestroyOnLoad(this.gameObject);
        if (Instance != null && Instance != this.gameObject)
        {
            //Destroy(this.gameObject);
        }
        else
        {
            Instance = this.gameObject;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = !aprovedConection;

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = aprovedConection;
        response.CreatePlayerObject = true;
    }
}
