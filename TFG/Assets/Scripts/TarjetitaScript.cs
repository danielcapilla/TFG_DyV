using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;
using TMPro;

public class TarjetitaScript : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> tarjetitaNameNetworkVariable = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public UserNetworkConfig userNetworkConfig;
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
        //Desuscribir al user del cambio de nombre de la tarjetita (si se cambia el nombre por lo que sea y no se hace peta)
        if (!IsServer) return;
        userNetworkConfig.usernameNetworkVariable.OnValueChanged -= CambiarTarjetitaName;
    }

    public void CambiarTarjetitaName(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        
        tarjetitaNameNetworkVariable.Value = newValue;
        GetComponentInChildren<TextMeshProUGUI>().text = tarjetitaNameNetworkVariable.Value.ToString();
    }
    
}
