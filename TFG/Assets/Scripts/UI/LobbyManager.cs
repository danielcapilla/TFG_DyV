using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI joinCodeTMP;
    [SerializeField]
    private GameObject tarjetitaPrefab;
    [SerializeField]
    private GameObject layout;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ShowJoinCode();
        if(IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            ShowUsersInfo();
        }
        
    }

    private void ShowUsersInfo()
    {
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            ShowUserInfo(id);
        }
    }

    private void OnClientDisconnected(ulong id)
    {
        TarjetitaScript[] tarjetitaArray = GameObject.FindObjectsOfType<TarjetitaScript>();
        foreach (TarjetitaScript tarjetita in tarjetitaArray)
        {
            if(tarjetita.GetComponent<NetworkObject>().OwnerClientId == id)
            {
                tarjetita.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        ShowUserInfo(clientId);
    }
    
    private void ShowJoinCode()
    {
        joinCodeTMP.text = TestRelay.staticCode;
    }
    private void ShowUserInfo(ulong id)
    {
        GameObject instance = Instantiate(tarjetitaPrefab);
        NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.SpawnWithOwnership(id);
        instance.transform.SetParent(layout.transform, false);
        TarjetitaScript tarjetita = instance.GetComponent<TarjetitaScript>();
        UserNetworkConfig userNetwork = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponent<UserNetworkConfig>();
        //Cambiamos el nombre de la tarjetita por el introducido en el login
        tarjetita.tarjetitaNameNetworkVariable.Value = userNetwork.usernameNetworkVariable.Value;
        tarjetita.profilePicIDNetworkVariable.Value = userNetwork.profilePicIDNetworkVariable.Value;
        //Para asegurarse de que el paso de nombre al user sucede antes que la tarjetita. 
        //Esto se hace sobre todo por la concurrencia y cuestiones de tiempo.
        userNetwork.usernameNetworkVariable.OnValueChanged += tarjetita.CambiarTarjetitaName;
        userNetwork.profilePicIDNetworkVariable.OnValueChanged += tarjetita.CambiarProfilePic;
        //Asignamos la referencia del userNetwork en la tarjetita para desuscribir
        tarjetita.userNetworkConfig = userNetwork;

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

    }

    public void ExitLobby() 
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }
}
