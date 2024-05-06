using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PodiumManager : NetworkBehaviour
{
    private TeamMenager teamManager;
    private LateJoinsBehaviour lateJoinsBehaviour;
    [SerializeField]
    private GameObject podium;
    [SerializeField]
    private Transform panelVG;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnUnload += UnSceceLoaded;
        teamManager = FindObjectOfType<TeamMenager>();
        lateJoinsBehaviour = FindObjectOfType<LateJoinsBehaviour>();

        ShowPodium();
    }
    private void ShowPodium()
    {
        ShowPodiumServerRPC();
        
        
    }
    [ServerRpc (RequireOwnership = false)]
    private void ShowPodiumServerRPC()
    {
        teamManager.teams.Sort((team1, team2) => team2.Puntuacion.CompareTo(team1.Puntuacion));
        for (int i = 0; i < teamManager.teams.Count; i++)
        {

            ShowPodiumClientRPC(i, teamManager.teams[i].ID, teamManager.teams[i].Puntuacion);
        }
    }
    [ClientRpc]
    private void ShowPodiumClientRPC(int position, int groupId, int score)
    {
        GameObject podiumGameObject = Instantiate(podium);
        podiumGameObject.transform.SetParent(panelVG);
        PodiumTarjetitaScript podiumScript = podiumGameObject.GetComponent<PodiumTarjetitaScript>();
        podiumScript.SetData($"{position + 1}º", $"{groupId}", $"{score}");
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkManager.Singleton.SceneManager.OnUnload -= UnSceceLoaded;
    }
    private void UnSceceLoaded(ulong clientId, string sceneName, AsyncOperation asyncOperation)
    {
        if (IsServer && sceneName == "Podium")
        {
            lateJoinsBehaviour.aprovedConection = true;
        }
    }
}
