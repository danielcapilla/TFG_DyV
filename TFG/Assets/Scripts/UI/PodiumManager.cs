using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PodiumManager : NetworkBehaviour
{
    private TeamMenager teamManager;
    private LateJoinsBehaviour lateJoinsBehaviour;
    [SerializeField]
    private GameObject podium;
    [SerializeField]
    private Transform panelVG;
    [SerializeField]
    private LocalizeStringEvent localizeStringEvent;
    [SerializeField]
    private GameObject lobbyButton;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        lobbyButton.SetActive(IsServer);
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.OnUnload += UnSceceLoaded;
        teamManager = FindFirstObjectByType<TeamMenager>();
        lateJoinsBehaviour = FindFirstObjectByType<LateJoinsBehaviour>();

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
        localizeStringEvent = podiumGameObject.GetComponentInChildren<LocalizeStringEvent>();
        var groupIdLocalizationString = localizeStringEvent.StringReference["groupId"] as IntVariable;
        groupIdLocalizationString.Value = groupId;
        podiumScript.SetData($"{position + 1}º", $"{score}");
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkManager.Singleton.SceneManager.OnUnload -= UnSceceLoaded;
    }
    private void UnSceceLoaded(ulong clientId, string sceneName, AsyncOperation asyncOperation)
    {
        Destroy(teamManager.gameObject);
        if (IsServer && sceneName == "Podium")
        {
            LateJoinsBehaviour.aprovedConection = true;
        }
    }
    public void ToLobby()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

}
