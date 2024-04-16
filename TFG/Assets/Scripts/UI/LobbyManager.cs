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
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
        }
        
    }

    private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        
        //Destruir los Players al volver al Lobby (si hace falta)
        if (GameObject.FindGameObjectsWithTag("Player") != null)
        {
            GameObject[] arrayFighters = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject obj in arrayFighters)
            {
                Destroy(obj.gameObject);

            }
        }
        if (IsServer && sceneName == "MinijuegoRestaurante")
        {
            Debug.Log("TETAS");
            foreach (ulong id in clientsCompleted)
            {
                Debug.Log($"Id generado... {id}");
                //Pongos los fighters como hijos del player
                //arrayPlayers[id].GetComponent<PlayerNetworkConfig>().InstantiateCharacterServerRpc(id);
                //NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<UserNetworkConfig>().InstantiatePlayerServerRpc(id);
                //PlayerNetworkConfig.Instance.InstantiateCharacterServerRpc(id);
                //player.transform.SetParent(transform, false);
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        ShowJoinCodeClientRPC();
       
    }
    [ClientRpc]
    private void ShowJoinCodeClientRPC()
    {
        joinCodeTMP.text = TestRelay.staticCode;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
    }

    public void IrAJuego()
    {

        NetworkManager.Singleton.SceneManager.LoadScene("MinijuegoRestaurante", LoadSceneMode.Single);
    }
}
