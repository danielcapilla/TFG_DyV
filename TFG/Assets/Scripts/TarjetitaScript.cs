using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;
using TMPro;

public class TarjetitaScript : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> tarjetitaNameNetworkVariable = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        tarjetitaNameNetworkVariable.Value = "";
        tarjetitaNameNetworkVariable.OnValueChanged += CambiarTarjetitaName;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        tarjetitaNameNetworkVariable.OnValueChanged -= CambiarTarjetitaName;
    }

    private void CambiarTarjetitaName(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        
        tarjetitaNameNetworkVariable.Value = newValue;
        GetComponentInChildren<TextMeshProUGUI>().text = tarjetitaNameNetworkVariable.Value.ToString();
    }
}
