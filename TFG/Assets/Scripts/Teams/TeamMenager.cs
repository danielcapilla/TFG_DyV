using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TeamMenager : NetworkBehaviour
{
    public int maxPlayers;
    public int maxplayersPerTeam;
    public int minPlayersPerTeam;
    public TeamInfo teamType;
    public List<TeamInfo> teams;
    public List<TeamInfo> teamsScoreSorted { get; private set; }
    public static TeamMenager Instance { get; private set; }

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    void Start()
    {
        // En online solo el servidor inicializa
        if (!IsOffline && !IsServer) return;

        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        int totalTeams = 6;
        for (int i = 0; i < totalTeams; i++)
        {
            TeamInfo copia = teamType.Clone();
            copia.ID = i + 1;
            teams.Add(copia);
        }
        teamsScoreSorted = new List<TeamInfo>(teams);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void QuitPlayerFromTheTeamServerRPC(ulong id, int groupNumber)
    {
        teams[groupNumber].integrantes.Remove(id);
    }

    public (int, int) GetPositions(TeamInfo teamThatScored)
    {
        int idx = teamsScoreSorted.IndexOf(teamThatScored);
        int idxold = idx;
        for (int i = idx; i >= 0; i--)
        {
            if (teamsScoreSorted[i].Puntuacion < teamThatScored.Puntuacion)
                idx = i;
        }
        return (idx, idxold);
    }

    public void SortTeams()
    {
        teamsScoreSorted.Sort((t1, t2) => t2.Puntuacion.CompareTo(t1.Puntuacion));
    }

    public void UpdateIndex(int deDondeVengo, int aDondeVoy)
    {
        TeamInfo temp = teamsScoreSorted[deDondeVengo];
        teamsScoreSorted.Remove(temp);
        teamsScoreSorted.Insert(aDondeVoy, temp);
    }
}
