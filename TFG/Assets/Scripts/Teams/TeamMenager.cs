using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TeamMenager : NetworkBehaviour
{
    public int maxPlayers;
    public int playersPerTeam;
    public TeamInfo teamType;
    public List<TeamInfo> teams;
    private Dictionary<ulong, bool> playerReadyDictionary;
    [SerializeField]
    private GameObject groupCanvas;
    
    // Start is called before the first frame update
    void Start()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
        if (!IsServer) return;
        
        int totalTeams = maxPlayers / playersPerTeam;
        for (int i = 0; i < totalTeams; i++) 
        {
            TeamInfo copia = teamType.Clone();
            copia.ID = i;
            teams.Add(copia);
        }
    }

    public void SetPlayerReady(ulong id)
    {
        playerReadyDictionary[id] = true;
        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }
        if (allClientsReady)
        {
            DesactivateGroupCanvasClientRPC();
            foreach (ulong playerId in NetworkManager.ConnectedClientsIds)
            {
                PlayerInput playerInput = NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponentInChildren<PlayerInput>();
                ActivatePlayerInputClientRPC(playerInput.GetComponent<NetworkObject>());
            }
        }
    }
    [ClientRpc]
    private void ActivatePlayerInputClientRPC(NetworkObjectReference playerInputNetworkObjectReference)
    {
        playerInputNetworkObjectReference.TryGet(out NetworkObject playerInputNetworkObject);
        PlayerController playerController = playerInputNetworkObject.GetComponent<PlayerController>();
        playerController.enabled = true;
    }
    [ClientRpc]
    public void DesactivateGroupCanvasClientRPC()
    {
        groupCanvas.SetActive(false);
    }
    [ServerRpc (RequireOwnership = false)]
    public void QuitPlayerFromTheTeamServerRPC(ulong id, int groupNumber)
    {
        teams[groupNumber].integrantes.Remove(id);
    }
}
