using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;
using TMPro;
using UnityEngine.UI;

public class TarjetitaScript : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> tarjetitaNameNetworkVariable = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> profilePicIDNetworkVariable = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public UserNetworkConfig userNetworkConfig;
    [SerializeField]
    private TextMeshProUGUI textMeshProUGUI;
    [SerializeField]
    private Image image;

    public ProfileImageList lobbyManager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //lobbyManager = FindAnyByType<ProfileImageList>();
        //Se tiene que ver tanto en el server (original) como en el owner (clone)
        tarjetitaNameNetworkVariable.OnValueChanged += CambiarTarjetitaName;
        profilePicIDNetworkVariable.OnValueChanged += CambiarProfilePic;
        //Si ya tenía nombre puesto, escribe el nombre que ya tenía (nuevas conexiones)
        textMeshProUGUI.text = tarjetitaNameNetworkVariable.Value.ToString();
        image.sprite = lobbyManager.ProfilePics[profilePicIDNetworkVariable.Value];

        // Solo el servidor debe suscribirse a los cambios del usuario
        if (IsServer && userNetworkConfig != null)
        {
            userNetworkConfig.usernameNetworkVariable.OnValueChanged += OnUsernameChanged;
            userNetworkConfig.profilePicIDNetworkVariable.OnValueChanged += OnProfilePicChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        tarjetitaNameNetworkVariable.OnValueChanged -= CambiarTarjetitaName;
        profilePicIDNetworkVariable.OnValueChanged -= CambiarProfilePic;
        //Desuscribir al user del cambio de nombre de la tarjetita (si se cambia el nombre por lo que sea y no se hace peta).
        //PARA EL CAMBIO DE ESCENA user se mantiene vivo pero tarjetita muere. Por si se cambia el nombre en partida.
        if (!IsServer || userNetworkConfig == null) return;
        userNetworkConfig.usernameNetworkVariable.OnValueChanged -= OnUsernameChanged;
        userNetworkConfig.profilePicIDNetworkVariable.OnValueChanged -= OnProfilePicChanged;
    }

    // Estos métodos solo actualizan la UI localmente cuando cambian las NetworkVariables
    private void CambiarTarjetitaName(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        textMeshProUGUI.text = newValue.ToString();
    }

    private void CambiarProfilePic(int previousValue, int newValue)
    {
        image.sprite = lobbyManager.ProfilePics[newValue];
    }

    // Métodos que solo el servidor ejecuta cuando cambian los valores del usuario
    private void OnUsernameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        // Solo el servidor puede modificar las NetworkVariables
        tarjetitaNameNetworkVariable.Value = newValue;
    }

    private void OnProfilePicChanged(int previousValue, int newValue)
    {
        // Solo el servidor puede modificar las NetworkVariables
        profilePicIDNetworkVariable.Value = newValue;
    }

    // Método para que el LobbyManager asigne la referencia al userNetworkConfig
    public void SetUserNetworkConfig(UserNetworkConfig config)
    {
        userNetworkConfig = config;

        // Si ya estamos spawnados, nos suscribimos a los cambios
        if (IsServer && IsSpawned && userNetworkConfig != null)
        {
            userNetworkConfig.usernameNetworkVariable.OnValueChanged += OnUsernameChanged;
            userNetworkConfig.profilePicIDNetworkVariable.OnValueChanged += OnProfilePicChanged;
        }
    }
}