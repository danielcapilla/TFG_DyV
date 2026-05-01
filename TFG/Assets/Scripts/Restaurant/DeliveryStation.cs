using DG.Tweening;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DeliveryStation : InteractableObject, IScoreEvent
{
    public event System.Action<int> OnScorePoint;

    [SerializeField] RecipeRandomizer randomizer;
    [SerializeField] Transform endPos;
    [SerializeField] Transform placePosition;
    [SerializeField] float time;
    ICarryObject holdingObject;
    [SerializeField] TeamManager teamMenager;

    [SerializeField] private StatisticsBehaviour statisticsBehaviour;
    [SerializeField] AudioSource ScoreSound;
    [SerializeField] AudioSource FailSound;
    private int offlineOrderIndex = 0; // Indice del pedido actual en modo offline

    // ── Offline ───────────────────────────────────────────────────────────────
    protected override void InteractOffline(GameObject player)
    {
        PlayerCarry carry = player.GetComponent<PlayerCarry>();
        if (carry == null || !carry.isCarrying) return;
        if (!carry.carryingObject.GetGameObject().TryGetComponent<PlateBehaviour>(out PlateBehaviour plate)) return;

        holdingObject = carry.DropObject();
        PlayerCarry.SetParentSafe(holdingObject.GetGameObject(), transform);
        holdingObject.GetGameObject().transform.localPosition = placePosition.localPosition;

        bool correct = EvaluatePlate(plate, 0);
        if (correct)
        {
            offlineOrderIndex++;
            OnScorePoint?.Invoke(1);
            ScoreSound?.Play();
            // Mostrar el siguiente pedido si quedan
            if (offlineOrderIndex < randomizer.currentOrders.Count)
                randomizer.NextOrder(offlineOrderIndex);
        }
        else
        {
            FailSound?.Play();
        }

        // Animar y destruir
        ICarryObject captured = holdingObject;
        captured.GetGameObject().transform
            .DOMove(endPos.position, time)
            .SetEase(Ease.InQuart)
            .OnComplete(() =>
            {
                foreach (ICarryObject obj in plate.GetGameObject()
                             .transform.GetComponentsInChildren<ICarryObject>().Reverse())
                    Destroy(obj.GetGameObject());
                Destroy(captured.GetGameObject());
            });
    }

    // ── Online ────────────────────────────────────────────────────────────────
    protected override void InteractOnline(GameObject player)
    {
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj != null) DeliverPlateServerRPC(netObj);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DeliverPlateServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry playerCarry = playerNet.GetComponent<PlayerCarry>();
        PlayerStats playerStats = playerNet.GetComponentInParent<PlayerStats>();
        if (!playerCarry.isCarrying) return;
        if (!playerCarry.carryingObject.GetGameObject().TryGetComponent<PlateBehaviour>(out PlateBehaviour plate)) return;

        PlaceOrderClientRPC(playerRef);
        MovePlateClientRPC(plate.GetNetworkObject());

        int idGrupo = playerStats.idGrupo.Value;
        bool correct = EvaluatePlate(plate, idGrupo);
        TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamMenager.teams[idGrupo];

        var deliveredBurguer = new DeliveredBurguerInfo
        {
            burguer = plate.Ingredients,
            idOrder = teamInfo.idOrder
        };
        teamInfo.Burguers.Add(deliveredBurguer);

        if (correct)
        {
            teamInfo.Puntuacion++;
            teamInfo.onPuntuacionChanged?.Invoke(teamInfo.Puntuacion);
            teamInfo.idOrder++;
            teamInfo.OnIdOrderChange?.Invoke(teamInfo.idOrder);
            NextOrderClientRpc(teamInfo.idOrder, teamInfo.Puntuacion, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = teamInfo.integrantes.ToArray() }
            });
            StartCoroutine(ChangeStatistics(teamInfo));
        }
        else
        {
            FailOrderClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = teamInfo.integrantes.ToArray() }
            });
        }
    }

    // ── Logica comun de evaluacion ────────────────────────────────────────────
    private bool EvaluatePlate(PlateBehaviour plate, int idGrupo)
    {
        if (randomizer == null || randomizer.currentOrders == null ||
            randomizer.currentOrders.Count == 0) return false;

        int orderIndex = IsOffline ? offlineOrderIndex : ((TeamInfoRestaurante)teamMenager.teams[idGrupo]).idOrder;
        if (orderIndex >= randomizer.currentOrders.Count) return false;

        var order = randomizer.currentOrders[orderIndex];
        if (order.Count != plate.Ingredients.Count) return false;

        for (int i = 0; i < order.Count; i++)
        {
            if (order[i].ID != plate.Ingredients[i].ingredient.ID)
                return false;
        }
        return true;
    }

    [ClientRpc]
    public void NextOrderClientRpc(int order, int teamScore, ClientRpcParams p = default)
    {
        OnScorePoint?.Invoke(1);
        ScoreSound.Play();
        if (order < randomizer.currentOrders.Count)
            randomizer.NextOrder(order);
    }

    [ClientRpc]
    public void FailOrderClientRpc(ClientRpcParams p = default) => FailSound.Play();

    [ClientRpc]
    private void PlaceOrderClientRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry playerCarry = playerNet.GetComponent<PlayerCarry>();
        holdingObject = playerCarry.DropObject();
        SetParentServerRPC();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetParentServerRPC()
    {
        PlayerCarry.SetParentSafe(holdingObject.GetGameObject(), transform);
    }

    [ClientRpc]
    public void MovePlateClientRPC(NetworkObjectReference plateRef)
    {
        if (!plateRef.TryGet(out NetworkObject plateNet)) return;
        PlateBehaviour plate = plateNet.GetComponent<PlateBehaviour>();

        holdingObject.GetGameObject().transform.localPosition = placePosition.localPosition;
        holdingObject.GetGameObject().transform
            .DOMove(endPos.position, time)
            .SetEase(Ease.InQuart)
            .OnComplete(() =>
            {
                DOTween.Kill(holdingObject.GetGameObject().transform);
                foreach (ICarryObject obj in plate.GetGameObject()
                             .transform.GetComponentsInChildren<ICarryObject>().Reverse())
                    obj.GetNetworkObject().Despawn(obj.GetGameObject());
            });
    }

    private IEnumerator ChangeStatistics(TeamInfoRestaurante teamInfo)
    {
        yield return new WaitUntil(() => statisticsBehaviour.finished);
        (int, int) pos = teamMenager.GetPositions(teamInfo);
        statisticsBehaviour.ChangePosition(pos.Item1, pos.Item2, teamMenager.teams.IndexOf(teamInfo));
    }
}
