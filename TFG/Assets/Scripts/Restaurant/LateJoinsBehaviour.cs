using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LateJoinsBehaviour :NetworkBehaviour
{
    
    public bool aprovedConection = true;
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        if (!IsServer) return;
        //NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }
    private void OnDestroy()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;

    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = aprovedConection;
        response.CreatePlayerObject = true;
    }
}
