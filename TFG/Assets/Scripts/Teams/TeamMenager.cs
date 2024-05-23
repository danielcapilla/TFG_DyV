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
    private List<TeamInfo> teamsScoreSorted;
    void Start()
    {
        
        if (!IsServer) return;
        DontDestroyOnLoad(gameObject);
        int totalTeams = maxPlayers / playersPerTeam;
        for (int i = 0; i < totalTeams; i++) 
        {
            TeamInfo copia = teamType.Clone();
            copia.ID = i;
            teams.Add(copia);
        }
        teamsScoreSorted = new List<TeamInfo>(teams);
    }
    [ServerRpc (RequireOwnership = false)]
    public void QuitPlayerFromTheTeamServerRPC(ulong id, int groupNumber)
    {
        teams[groupNumber].integrantes.Remove(id);
    }

    public void SortTeams(TeamInfo teamThatScored)
    {
        int idx = teamsScoreSorted.IndexOf(teamThatScored);
        for (int i = idx; i > 0; i--)
        {
            if (teamsScoreSorted[i].Puntuacion < teamThatScored.Puntuacion)
            {
                teamsScoreSorted.RemoveAt(idx);
                teamsScoreSorted.Insert(i, teamThatScored);
            }
        }
        teamsScoreSorted.Sort((a, b) => b.Puntuacion.CompareTo(a.Puntuacion));
    }
}
