using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Gestiona los equipos de cualquier minijuego.
/// totalTeams y maxplayersPerTeam se configuran en el inspector
/// y ChooseGroup los lee para generar la UI dinamicamente.
/// </summary>
public class TeamManager : NetworkBehaviour
{
    [Header("Configuracion de equipos")]
    public int totalTeams = 6;
    public int maxPlayersPerTeam = 4;
    public int minPlayersPerTeam = 1;

    [Header("Tipo de equipo (ScriptableObject del juego)")]
    public TeamInfo teamType;

    public List<TeamInfo> teams = new List<TeamInfo>();
    public List<TeamInfo> teamsScoreSorted { get; private set; }

    public static TeamManager Instance { get; private set; }

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    void Start()
    {
        if (!IsOffline && !IsServer) return;

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        teams.Clear();
        for (int i = 0; i < totalTeams; i++)
        {
            TeamInfo copy = teamType.Clone();
            copy.ID = i;
            teams.Add(copy);
        }
        teamsScoreSorted = new List<TeamInfo>(teams);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void QuitPlayerFromTheTeamServerRPC(ulong id, int groupNumber)
    {
        if (groupNumber >= 0 && groupNumber < teams.Count)
            teams[groupNumber].integrantes.Remove(id);
    }

    public (int, int) GetPositions(TeamInfo teamThatScored)
    {
        int idx = teamsScoreSorted.IndexOf(teamThatScored);
        int idxOld = idx;
        for (int i = idx - 1; i >= 0; i--)
            if (teamsScoreSorted[i].Puntuacion < teamThatScored.Puntuacion)
                idx = i;
        return (idx, idxOld);
    }

    public void SortTeams() =>
        teamsScoreSorted.Sort((t1, t2) => t2.Puntuacion.CompareTo(t1.Puntuacion));

    public void UpdateIndex(int from, int to)
    {
        TeamInfo temp = teamsScoreSorted[from];
        teamsScoreSorted.Remove(temp);
        teamsScoreSorted.Insert(to, temp);
    }
}
