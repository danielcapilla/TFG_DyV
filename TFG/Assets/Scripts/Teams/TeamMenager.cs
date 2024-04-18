using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TeamMenager : MonoBehaviour
{
    public int maxPlayers;
    public int playersPerTeam;
    public TeamInfo teamType;
    public List<TeamInfo> teams;

    // Start is called before the first frame update
    void Start()
    {
        int totalTeams = maxPlayers / playersPerTeam;
        for (int i = 0; i < totalTeams; i++) 
        {
            TeamInfo copia = teamType.clone();
            copia.ID = i;
            teams.Add(copia);
        }
    }

}
