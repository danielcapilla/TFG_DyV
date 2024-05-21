using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public NetworkVariable<int> idGrupo = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        idGrupo.OnValueChanged += NewIdGrupo;
        idGrupo.Value = -1;
    }
    public override void OnNetworkDespawn()
    {
        idGrupo.OnValueChanged -= NewIdGrupo;
    }
    private void NewIdGrupo(int previousValue, int newValue)
    {
        idGrupo.Value = newValue;
    }
}
