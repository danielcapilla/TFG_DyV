using DG.Tweening;
using System;
using Unity.Netcode;
using UnityEngine;

public class TurnBehaviour : NetworkBehaviour
{
    [Header("Paneles")]
    [SerializeField] private CanvasGroup movementPanel;
    [SerializeField] private CanvasGroup waitingPanel;

    [Header("Eventos")]
    [SerializeField] private GroupBehaviour groupBehaviour;
    [SerializeField] private ChooseGroup chooseGroup;
    [SerializeField] private GameManagerChicken gameManager;
    [SerializeField] private TurnTimer turnTimer;
    [SerializeField] private TeamMenager teamMenager;

    private bool isInitializingTurn = false;
    private Tween activateDelayTween;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        groupBehaviour.OnCommandAdded += NextTurn;
        turnTimer.OnTimerEnd += ResetTurn;
        //Debug.Log(Clie);
        if (!IsServer) return;
        gameManager.OnPlayerSpawned += HandlePlayerSpawned;
        groupBehaviour.OnExecutedTurn += HandleOnExecutedTurn;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        groupBehaviour.OnCommandAdded -= NextTurn;
        turnTimer.OnTimerEnd -= ResetTurn;
        if (!IsServer) return;
        gameManager.OnPlayerSpawned -= HandlePlayerSpawned;
        groupBehaviour.OnExecutedTurn -= HandleOnExecutedTurn;
    }
    private void KillTweens()
    {
        DOTween.Kill(waitingPanel);
        if (waitingPanel != null) DOTween.Kill(waitingPanel.transform);
        DOTween.Kill(movementPanel);
        if (movementPanel != null) DOTween.Kill(movementPanel.transform);
        activateDelayTween?.Kill();
    }
    private void OnDestroy()
    {
        KillTweens();
    }
    private void OnDisable()
    {
        KillTweens();
    }
    private void HandleOnExecutedTurn(PlayerInputController controller, int arg2)
    {
        // Por si a la hora de pasar al podio entra a ejecutar turno (lo mismo en TurnTimer)
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            return;

        var netObj = controller.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.IsSpawned)
            return;

        HandlePlayerSpawned(netObj, arg2);
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

        waitingPanel.gameObject.SetActive(true);
        movementPanel.gameObject.SetActive(true);


        waitingPanel.alpha = 0;
        waitingPanel.interactable = false;
        waitingPanel.blocksRaycasts = false;


        movementPanel.alpha = 0;
        movementPanel.interactable = false;
        movementPanel.blocksRaycasts = false;
        movementPanel.transform.localScale = Vector3.one * 1.05f;


        activateDelayTween?.Kill();
        activateDelayTween = DOVirtual.DelayedCall(0.03f, ShowMoves);
    }

    public void NextTurn(ulong id, CommandType commandType)
    {
        if (movementPanel.interactable)
        {
            ShowWaiting();
        }   
        else
        {

            ShowMoves();
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
            groupBehaviour.ExecuteTurn(idGroup);
        }
        else
        {
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

    public void ResetTurn(ulong id)
    {
        waitingPanel.alpha = 1;
        waitingPanel.interactable = false;
        waitingPanel.blocksRaycasts = false;

        movementPanel.alpha = 0;
        movementPanel.interactable = false;
        movementPanel.blocksRaycasts = false;
        int idGroup = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerStats>().idGrupo.Value;
        ResetTurnRPC(idGroup);
    }

    [Rpc(SendTo.Server)]
    private void ResetTurnRPC(int idGroup)
    {
        TeamInfoChicken teamInfo = (TeamInfoChicken)teamMenager.teams[idGroup];
        teamInfo.turn = 0;
        groupBehaviour.ExecuteTurn(idGroup);
        //ulong nextClientId = teamInfo.integrantes[teamInfo.turn % teamInfo.integrantes.Count];
        //ActivatePanelsClientRPC(new ClientRpcParams
        //{
        //    Send = new ClientRpcSendParams
        //    {
        //        TargetClientIds = new[] { nextClientId }
        //    }
        //});
    }
    // Transiciones UI
    private void ShowMoves()
    {
        // Cancelamos animaciones anteriores para evitar mezclas
        DOTween.Kill(waitingPanel);
        DOTween.Kill(movementPanel);

        // Fade out waiting
        waitingPanel.DOFade(0, 0.25f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            waitingPanel.interactable = false;
            waitingPanel.blocksRaycasts = false;
        });
        waitingPanel.transform.DOScale(0.95f, 0.25f).SetEase(Ease.InOutSine);

        // Fade in movement
        movementPanel.gameObject.SetActive(true);
        movementPanel.alpha = 0;
        movementPanel.transform.localScale = Vector3.one * 1.05f;

        movementPanel.DOFade(1, 0.3f).SetDelay(0.25f).SetEase(Ease.OutSine);
        movementPanel.transform.DOScale(1f, 0.3f).SetDelay(0.25f).SetEase(Ease.OutBack)
            .OnStart(() =>
            {
                movementPanel.interactable = true;
                movementPanel.blocksRaycasts = true;
            });
    }

    private void ShowWaiting()
    {
        // Cancelamos animaciones anteriores
        DOTween.Kill(waitingPanel);
        DOTween.Kill(movementPanel);
        movementPanel.interactable = false;
        movementPanel.blocksRaycasts = false;
        // Fade out movement
        movementPanel.DOFade(0, 0.25f).SetEase(Ease.InOutSine);
        movementPanel.transform.DOScale(0.95f, 0.25f).SetEase(Ease.InOutSine);

        // Fade in waiting
        waitingPanel.gameObject.SetActive(true);
        waitingPanel.alpha = 0;
        waitingPanel.transform.localScale = Vector3.one * 1.05f;

        waitingPanel.DOFade(1, 0.3f).SetDelay(0.25f).SetEase(Ease.OutSine);
        waitingPanel.transform.DOScale(1f, 0.3f).SetDelay(0.25f).SetEase(Ease.OutBack)
            .OnStart(() =>
            {
                waitingPanel.interactable = true;
                waitingPanel.blocksRaycasts = true;
            });
    }
}
