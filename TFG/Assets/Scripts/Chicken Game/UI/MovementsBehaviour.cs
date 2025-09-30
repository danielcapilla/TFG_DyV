using System;
using System.Data;
using Unity.Netcode;
using UnityEngine;
public class MovementsBehaviour : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject Movement;
    [SerializeField] private GameObject HorizontalLayout;
    [SerializeField] private GroupBehaviour groupBehaviour;
    [SerializeField] private TeamMenager teamMenager;

    override public void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        groupBehaviour.OnCommandAdded += HandleCommandAdded;
        if (IsServer)
        {
            
        }
    }
    override public void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        groupBehaviour.OnCommandAdded -= HandleCommandAdded;
        if (IsServer)
        {
            
        }
    }
    [Rpc(SendTo.Server)]
    private void HandleSpawnRPC(int idGroup, CommandType commandType)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        SpawnMoveForClientsClientRPC(commandType, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = teamInfo.integrantes.ToArray()
            }
        });
    }
    private void HandleCommandAdded(ulong id, CommandType commandType)
    {
        int idGrupo = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        HandleSpawnRPC(idGrupo, commandType);          
    }
    [ClientRpc]
    private void SpawnMoveForClientsClientRPC(CommandType commandType, ClientRpcParams clientRpcParams = default)
    {
        GameObject move = Instantiate(Movement, HorizontalLayout.transform);
        int childIndex = commandType switch
        {
            CommandType.MoveUp => 0,
            CommandType.MoveDown => 1,
            CommandType.MoveLeft => 2,
            CommandType.MoveRight => 3,
            CommandType.Wait => 4,
            _ => -1
        };
        move.transform.GetChild(childIndex).gameObject.SetActive(true);
    }
}
