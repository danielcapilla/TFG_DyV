using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;

public class TeamMenager : NetworkBehaviour
{
    public int maxPlayers;
    public int maxplayersPerTeam;
    public int minPlayersPerTeam;
    public TeamInfo teamType;
    public List<TeamInfo> teams;
    public List<TeamInfo> teamsScoreSorted { get; private set; }
    public static TeamMenager Instance { get; private set; }
    void Start()
    {
        
        if (!IsServer) return;
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        //int totalTeams = maxPlayers / maxplayersPerTeam;
        int totalTeams = 6;
        for (int i = 0; i < totalTeams; i++) 
        {
            TeamInfo copia = teamType.Clone();
            copia.ID = i+1;
            teams.Add(copia);
        }
        teamsScoreSorted = new List<TeamInfo>(teams);
    }
    // Singleton
    //void Awake()
    //{
    //    //if (!IsServer) return;
    //    if (Instance != null && Instance != this)
    //    {
    //        Destroy(this.gameObject);
    //        return;
    //    }
    //    Instance = this;
    //   // DontDestroyOnLoad(this.gameObject);
    //}
    [ServerRpc (RequireOwnership = false)]
    public void QuitPlayerFromTheTeamServerRPC(ulong id, int groupNumber)
    {
        teams[groupNumber].integrantes.Remove(id);
    }

    public (int,int) GetPositions(TeamInfo teamThatScored)
    {
        int idx = teamsScoreSorted.IndexOf(teamThatScored);
        int idxold = idx;
        for (int i = idx; i >= 0; i--)
        {
            if (teamsScoreSorted[i].Puntuacion < teamThatScored.Puntuacion)
            {
                //teamsScoreSorted.RemoveAt(idx);
                //teamsScoreSorted.Insert(i, teamThatScored);
                idx = i;

            }
        }
        
        return (idx, idxold);
    }
    public void SortTeams()
    {
        teamsScoreSorted.Sort((team1, team2) => team2.Puntuacion.CompareTo(team1.Puntuacion));
    }
    public void UpdateIndex(int deDondeVengo, int aDondeVoy)
    {
        TeamInfo temp = teamsScoreSorted[deDondeVengo];
        teamsScoreSorted.Remove(temp);
        teamsScoreSorted.Insert(aDondeVoy, temp);
    }
}
