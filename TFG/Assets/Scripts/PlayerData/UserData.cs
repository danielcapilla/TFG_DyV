using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UserData : NetworkBehaviour
{
    public NetworkVariable<string> Username;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Username.OnValueChanged += updateName;
        Username.Value = PlayerData.Name;
    }

    private void updateName(string previousValue, string newValue)
    {
        Username.Value = newValue;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Username.OnValueChanged -= updateName;
    }
}
