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
        if (IsServer)
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
            if (id == OwnerClientId) continue;
            ShowUserInfo(id);
        }
    }

    private void OnClientDisconnected(ulong id)
    {
        TarjetitaScript[] tarjetitaArray = GameObject.FindObjectsByType<TarjetitaScript>(FindObjectsSortMode.None);
        foreach (TarjetitaScript tarjetita in tarjetitaArray)
        {
            if (tarjetita.GetComponent<NetworkObject>().OwnerClientId == id)
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

        // Asignamos la referencia del userNetwork en la tarjetita
        tarjetita.SetUserNetworkConfig(userNetwork);

        // Solo el servidor puede modificar las NetworkVariables
        if (IsServer)
        {
            //Cambiamos el nombre de la tarjetita por el introducido en el login
            tarjetita.tarjetitaNameNetworkVariable.Value = userNetwork.usernameNetworkVariable.Value;
            tarjetita.profilePicIDNetworkVariable.Value = userNetwork.profilePicIDNetworkVariable.Value;
        }
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
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
}