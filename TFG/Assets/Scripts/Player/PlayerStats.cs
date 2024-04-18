using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public NetworkVariable<int> idGrupo = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        idGrupo.OnValueChanged += NewIdGrupo;
        idGrupo.Value = 0;
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
