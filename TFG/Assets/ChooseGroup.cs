using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChooseGroup : NetworkBehaviour
{
    PlayerStats player;
    [SerializeField] TeamMenager teamMenager;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        
    }
    public void Cambio()
    {
        CambioServerRPC(NetworkManager.Singleton.LocalClientId);
    }
    [ServerRpc (RequireOwnership = false)]
    private void CambioServerRPC(ulong id)
    {
        player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject.GetComponentInChildren<PlayerStats>();
        player.idGrupo.Value = 1;
        TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamMenager.teams[player.idGrupo.Value];
        teamInfo.integrantes.Add(id);
    }
}
