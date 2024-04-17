using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;
using TMPro;

public class TarjetitaScript : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> tarjetitaNameNetworkVariable = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public UserNetworkConfig userNetworkConfig;
    [SerializeField]
    private TextMeshProUGUI textMeshProUGUI;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //Se tiene que ver tanto en el server (original) como en el owner (clone)
        tarjetitaNameNetworkVariable.OnValueChanged += CambiarTarjetitaName;
        //Si ya tenía nombre puesto, escribe el nombre que ya tenía (nuevas conexiones)
        textMeshProUGUI.text = tarjetitaNameNetworkVariable.Value.ToString();       
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        tarjetitaNameNetworkVariable.OnValueChanged -= CambiarTarjetitaName;
        //Desuscribir al user del cambio de nombre de la tarjetita (si se cambia el nombre por lo que sea y no se hace peta).
        //PARA EL CAMBIO DE ESCENA user se mantiene vivo pero tarjetita muere. Por si se cambia el nombre en partida.
        if (!IsServer) return;
        userNetworkConfig.usernameNetworkVariable.OnValueChanged -= CambiarTarjetitaName;
    }

    public void CambiarTarjetitaName(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        
        tarjetitaNameNetworkVariable.Value = newValue;
        textMeshProUGUI.text = tarjetitaNameNetworkVariable.Value.ToString();
    }
    
}
