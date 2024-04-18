using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public NetworkVariable<int> idGrupo = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        idGrupo.OnValueChanged += NewIdGrupo;
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
