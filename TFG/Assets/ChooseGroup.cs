using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChooseGroup : NetworkBehaviour
{
    PlayerStats player;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        
    }

    public void Cambio()
    {
        //if (!IsOwner) return;
        player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponentInChildren<PlayerStats>();
        Debug.Log(NetworkManager.Singleton.LocalClientId);
        player.idGrupo.Value = 1;
    }
}
