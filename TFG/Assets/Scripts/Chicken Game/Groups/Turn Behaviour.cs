using DG.Tweening.Core.Easing;
using System;
using System.Collections;
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
    [SerializeField] private TurnTimer turnTimer;
    [SerializeField] private TeamMenager teamMenager;

    //public NetworkVariable<int> turn = new NetworkVariable<int>(0,
    //    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        groupBehaviour.OnCommandAdded += NextTurn;
        //groupBehaviour.OnExecutedTurn += NextTurn;
        turnTimer.OnTimerEnd += ResetTurn;

        //chooseGroup.OnGameStartEvent += ActivatePanelsRPC(NetworkManager.Singleton.LocalClientId);
        if (!IsServer) return;
        gameManager.OnPlayerSpawned += HandlePlayerSpawned;
        groupBehaviour.OnExecutedTurn += HandleOnExecutedTurn;
        //turn.OnValueChanged += TurnChange;

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        groupBehaviour.OnCommandAdded -= NextTurn;
        //groupBehaviour.OnExecutedTurn -= NextTurn;
        turnTimer.OnTimerEnd -= ResetTurn;
        if (!IsServer) return;
        gameManager.OnPlayerSpawned -= HandlePlayerSpawned;
        groupBehaviour.OnExecutedTurn -= HandleOnExecutedTurn;
    }
    private void HandleOnExecutedTurn(PlayerInputController controller, int arg2)
    {
        HandlePlayerSpawned(controller.NetworkObject, arg2);
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
    private void ActivatePanelsClientRPC( ClientRpcParams clientRpcParams = default)
    {
            SetPanelsActive();
    }

    private void SetPanelsActive()
    {
        movementPanel.SetActive(true);
        waitingPanel.SetActive(false);
    }
    public void NextTurn(ulong id, CommandType commandType)
    {
        if (movementPanel.activeSelf)
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
        bool delay = false;
        if (teamInfo.turn >= teamInfo.integrantes.Count)
        {
            teamInfo.turn = 0;
            delay = true;
            groupBehaviour.ExecuteTurn(idGroup);
        }
        else
        {
            // Si no es el ultimo, solo activar los paneles del siguiente
            ulong nextClientId = teamInfo.integrantes[teamInfo.turn % teamInfo.integrantes.Count];
            ActivatePanelsClientRPC(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { nextClientId }
                }
            });
        }

        return;
    }
    public void ResetTurn(ulong id)
    {
        movementPanel.SetActive(false);
        waitingPanel.SetActive(true);
        int idGroup = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        ResetTurnRPC(idGroup);
    }
    [Rpc(SendTo.Server)]
    private void ResetTurnRPC(int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        teamInfo.turn = 0;
        groupBehaviour.ExecuteTurn(idGroup);
        ulong nextClientId = teamInfo.integrantes[teamInfo.turn % teamInfo.integrantes.Count];
        ActivatePanelsClientRPC(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { nextClientId }
            }
        });
    }

}
