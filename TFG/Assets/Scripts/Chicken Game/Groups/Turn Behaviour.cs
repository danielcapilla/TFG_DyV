using DG.Tweening.Core.Easing;
using System;
using Unity.Netcode;
using UnityEngine;

public class TurnBehaviour : NetworkBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject movementPanel;
    [SerializeField] private GameObject waitingPanel;
    [Header("Eventos")]
    [SerializeField] private GroupBehaviour groupBehaviour;
    [SerializeField] private ChooseGroup chooseGroup;
    [SerializeField] private GameManagerChicken gameManager;
    [Header("Grupo")]
    [SerializeField] private TeamMenager teamMenager;

    //public NetworkVariable<int> turn = new NetworkVariable<int>(0,
    //    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        groupBehaviour.OnCommandAdded += NextTurn;
        //groupBehaviour.OnExecutedTurn += NextTurn;
        
        //chooseGroup.OnGameStartEvent += ActivatePanelsRPC(NetworkManager.Singleton.LocalClientId);
        if (!IsServer) return;
        gameManager.OnPlayerSpawned += HandlePlayerSpawned;
        //turn.OnValueChanged += TurnChange;

    }

    private void HandlePlayerSpawned(NetworkObjectReference playerNOR, int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        ulong targetClientId = teamInfo.integrantes[0];
        ActivatePanelsClientRPC(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { targetClientId }
            }
        });

    }

    [ClientRpc]
    private void ActivatePanelsClientRPC(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"Activando paneles para el cliente: {NetworkManager.Singleton.LocalClientId}");
        movementPanel.SetActive(true);
        waitingPanel.SetActive(false);
    }


    private void NextTurn(ulong id)
    {
        if(movementPanel.activeSelf)
        {
            movementPanel.SetActive(false);
            waitingPanel.SetActive(true);
        }
        else
        {
            movementPanel.SetActive(true);
            waitingPanel.SetActive(false);
        }
        ChangeTurnRPC(id);
    }
    [Rpc(SendTo.Server)]
    private void ChangeTurnRPC(ulong id)
    {
        int idGroup = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        teamInfo.turn++;
        if (teamInfo.turn >= teamInfo.integrantes.Count)
        {
            teamInfo.turn = 0;
        }

        // Si no es el ultimo, solo activar los paneles del siguiente
        ulong nextClientId = teamInfo.integrantes[teamInfo.turn % teamInfo.integrantes.Count];
        ActivatePanelsClientRPC(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { nextClientId }
            }
        });
        return;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsServer) return;
        groupBehaviour.OnCommandAdded -= NextTurn;
    }
}
