using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DeliveryStation : InteractableObject
{
    [SerializeField] RecipeRandomizer randomizer;
    [SerializeField] Transform endPos;
    [SerializeField] Transform placePosition;
    [SerializeField] float time;
    ICarryObject holdingObject;
    [SerializeField] TeamMenager teamMenager;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private StatisticsBehaviour statisticsBehaviour;

    [SerializeField] AudioSource ScoreSound;
    [SerializeField] AudioSource FailSound;

    [SerializeField] HamburgersInfo_Collector hamburgersInfo_Collector;

    private Timer timerTotal = new Timer();
    private Timer timerHamburguer = new Timer();

    private new void Start()
    {
        base.Start();
        timerTotal.StartTimer();
        timerHamburguer.StartTimer();
    }

    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);
        DeliverPlateServerRPC(player.GetNetworkObject());
        //Si lleva un plato

    }

    [ServerRpc(RequireOwnership = false)]
    public void DeliverPlateServerRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerCarry playerCarry = playerNetworkObject.GetComponent<PlayerCarry>();
        PlayerStats playerStats = playerNetworkObject.GetComponentInParent<PlayerStats>();

        if (playerCarry.carryingObject.GetGameObject().TryGetComponent<PlateBehaviour>(out PlateBehaviour plate))
        {
            //Colocar plato en superficie
            PlaceOrderClientRPC(playerNetworkObjectReference);
            //Animar plato (con DOTween puede que con alguna corutina o algo) para que sea entregado
            MovePlateClientRPC(plate.GetNetworkObject());
            //Evaluar plato respecto al pedido
            bool same = true;
            TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamMenager.teams[playerStats.idGrupo.Value];
            DeliveredBurguerInfo deliveredBurguer = new();
            deliveredBurguer.burguer = plate.Ingredients;
            deliveredBurguer.idOrder = teamInfo.idOrder;
            teamInfo.Burguers.Add(deliveredBurguer);
            if (randomizer.currentOrders[teamInfo.idOrder].Count == plate.Ingredients.Count)
            {
                for (int i = 0; i < randomizer.currentOrders[teamInfo.idOrder].Count; ++i)
                {
                    if (randomizer.currentOrders[teamInfo.idOrder][i].ID != plate.Ingredients[i].ingredient.ID)
                    {
                        same = false;
                    }
                }
            }
            else
            {
                same = false;
            }


            // Send info about the delivered hamburger to the collector
            if (!same)
            {
                hamburgersInfo_Collector.requestedHamburgers[teamInfo.idOrder].NumFails++;
            }
            else
            {
                Debug.Log("Timer hamburguer: " + timerHamburguer.GetElapsedTime() + " State: " + timerHamburguer.IsRunning());
                hamburgersInfo_Collector.requestedHamburgers[teamInfo.idOrder].TimeRequested = (int) Math.Round(timerHamburguer.GetElapsedTime());
                timerHamburguer.ResetTimer();

                hamburgersInfo_Collector.RequestedHamburguersToString();
            }

            Debug.Log($"Delivered hamburger for order {teamInfo.idOrder} | Correct: {same} " +
          $"| Time needed: {hamburgersInfo_Collector.requestedHamburgers[teamInfo.idOrder].TimeRequested} \n Requested at: " +
          $"{hamburgersInfo_Collector.requestedHamburgers[teamInfo.idOrder].MomentRequested} \n" +
          $"{hamburgersInfo_Collector.requestedHamburgers[teamInfo.idOrder].NumFails}");
            


            //Entregar puntuacion
            if (same)
            {
                Debug.Log("Hamburguesa correcta");
                teamInfo.Puntuacion++;
                teamInfo.onPuntuacionChanged?.Invoke(teamInfo.Puntuacion);
                teamInfo.idOrder++;
                teamInfo.OnIdOrderChange?.Invoke(teamInfo.idOrder);
                NextOrderClientRpc(teamInfo.idOrder, teamInfo.Puntuacion, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = teamInfo.integrantes.ToArray()
                    }
                });

                // Update statistics
                timerHamburguer.StartTimer();
                // Set the time that the hamburguer is requested
                Debug.Log("Timer total: " + timerTotal.GetElapsedTime() + "State: " + timerTotal.IsRunning());
                hamburgersInfo_Collector.requestedHamburgers[teamInfo.idOrder].MomentRequested = (int)Math.Round(timerTotal.GetElapsedTime());

                //Mutex Unity
                StartCoroutine(ChangeStatistics(teamInfo));
            }
            else
            {
                Debug.Log("Wrong deliver");
                FailOrderClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = teamInfo.integrantes.ToArray()
                    }
                });
            }

        }
    }

    [ClientRpc]
    public void NextOrderClientRpc(int order, int teamScore, ClientRpcParams clientRpcParams = default)
    {
        scoreText.text = teamScore.ToString();
        ScoreSound.Play();
        if(order < randomizer.currentOrders.Count)
        {
            randomizer.NextOrder(order);   
        }
    }

    [ClientRpc]
    public void FailOrderClientRpc(ClientRpcParams clientRpcParams = default)
    {
        FailSound.Play();
    }

    [ClientRpc]
    private void PlaceOrderClientRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerCarry playerCarry = playerNetworkObject.GetComponent<PlayerCarry>();
        holdingObject = playerCarry.DropObject();
        SetParentTableServerRPC();
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetParentTableServerRPC()
    {
        holdingObject.GetGameObject().transform.parent = this.transform;
    }
    [ClientRpc]
    public void MovePlateClientRPC(NetworkObjectReference plateNetworkObjectReference)
    {
        plateNetworkObjectReference.TryGet(out NetworkObject plateNetworkObject);
        PlateBehaviour plate = plateNetworkObject.GetComponent<PlateBehaviour>();

        holdingObject.GetGameObject().transform.localPosition = placePosition.localPosition;

        holdingObject.GetGameObject().transform.DOMove(endPos.position, time).SetEase(Ease.InQuart).OnComplete(() =>
        {
            DOTween.Kill(holdingObject.GetGameObject().transform);
            foreach (ICarryObject objToDestroy in plate.GetGameObject().transform.GetComponentsInChildren<ICarryObject>().Reverse())
            {
                objToDestroy.GetNetworkObject().Despawn(objToDestroy.GetGameObject());
            }
        });
    }

    private IEnumerator ChangeStatistics(TeamInfoRestaurante teamInfo)
    {
        //Mutex de unity
        yield return new WaitUntil(() => statisticsBehaviour.finished);
        (int, int) posiciones = teamMenager.GetPositions(teamInfo);
        statisticsBehaviour.ChangePosition(posiciones.Item1, posiciones.Item2, teamMenager.teams.IndexOf(teamInfo));
    }
}
