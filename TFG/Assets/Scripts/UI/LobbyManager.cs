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
    private GameObject button;
    [SerializeField]
    private GameObject tarjetitaPrefab;
    [SerializeField]
    private GameObject layout;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            button.SetActive(false);
        }
        if(IsServer)
        {
            joinCodeTMP.text = TestRelay.staticCode;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        
    }

   

    private void OnClientConnected(ulong clientId)
    {
        ShowJoinCodeClientRPC();
        ShowUserInfoClientRPC();
    }
    [ClientRpc]
    private void ShowJoinCodeClientRPC()
    {
        joinCodeTMP.text = TestRelay.staticCode;
    }
    [ClientRpc]
    private void ShowUserInfoClientRPC()
    {
        GameObject instance = Instantiate(tarjetitaPrefab);
        instance.transform.SetParent(layout.transform, false);
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    public void IrAJuego()
    {

        NetworkManager.Singleton.SceneManager.LoadScene("MinijuegoRestaurante", LoadSceneMode.Single);
    }
}
