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
    }
    [ServerRpc (RequireOwnership = false)]
    public void QuitPlayerFromTheTeamServerRPC(ulong id, int groupNumber)
    {
        teams[groupNumber].integrantes.Remove(id);
    }
}
