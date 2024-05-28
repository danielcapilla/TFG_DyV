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
    public int playersPerTeam;
    public TeamInfo teamType;
    public List<TeamInfo> teams;
    public List<TeamInfo> teamsScoreSorted { get; private set; }
    void Start()
    {
        
        if (!IsServer) return;
        DontDestroyOnLoad(gameObject);
        int totalTeams = maxPlayers / playersPerTeam;
        for (int i = 0; i < totalTeams; i++) 
        {
            TeamInfo copia = teamType.Clone();
            copia.ID = i+1;
            teams.Add(copia);
        }
        teamsScoreSorted = new List<TeamInfo>(teams);
    }
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
