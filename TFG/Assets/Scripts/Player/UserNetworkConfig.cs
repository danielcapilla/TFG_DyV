using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserNetworkConfig : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> usernameNetworkVariable = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        usernameNetworkVariable.OnValueChanged += UpdateName;
        usernameNetworkVariable.Value = PlayerData.Name;
    }

    private void UpdateName(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        usernameNetworkVariable.Value = newValue;
        Debug.Log("User: "+ usernameNetworkVariable.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if(!IsOwner) return;
        usernameNetworkVariable.OnValueChanged -= UpdateName;
    }
}
