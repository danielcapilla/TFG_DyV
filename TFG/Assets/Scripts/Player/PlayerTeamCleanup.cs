using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Gestiona la limpieza del equipo cuando el jugador abandona la partida.
/// Separado de PlayerCarry para respetar el principio de responsabilidad unica.
/// </summary>
public class PlayerTeamCleanup : NetworkBehaviour
{
    private TeamMenager teamManager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        var tm = GameObject.Find("TeamManager");
        if (tm != null) teamManager = tm.GetComponent<TeamMenager>();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        if (teamManager == null) return;

        PlayerStats stats = GetComponentInParent<PlayerStats>();
        if (stats == null) return;

        teamManager.QuitPlayerFromTheTeamServerRPC(OwnerClientId, stats.idGrupo.Value);
    }
}
