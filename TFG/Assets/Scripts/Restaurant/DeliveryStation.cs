using DG.Tweening;
using System;
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
        PlayerStats playerStats = playerNetworkObject.GetComponent<PlayerStats>();

        if (playerCarry.carryingObject.GetGameObject().TryGetComponent<PlateBehaviour>(out PlateBehaviour plate))
        {
            //Colocar plato en superficie
            PlaceOrderClientRPC(playerNetworkObjectReference);
            //Animar plato (con DOTween puede que con alguna corutina o algo) para que sea entregado
            MovePlateClientRPC(plate.GetNetworkObject());
            //Evaluar plato respecto al pedido
            bool same = true;
            TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamMenager.teams[playerStats.idGrupo.Value];
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
            //Entregar puntuacion
            Debug.Log("Entrego hamburguesa el equipo: " + teamInfo.ID);
            if (same)
            {
                Debug.Log("Hamburguesa correcta");
                teamInfo.Puntuacion++;
                teamInfo.onPuntuacionChanged.Invoke(teamInfo.Puntuacion);
            }
            else
            {
                Debug.Log("La has cagado....");
            }
            teamInfo.idOrder++;
            NextOrderClientRpc(teamInfo.idOrder, teamInfo.Puntuacion, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = teamInfo.integrantes.ToArray()
                }
            });
        }
    }

    [ClientRpc]
    public void NextOrderClientRpc(int order, int teamScore, ClientRpcParams clientRpcParams = default)
    {
        //playerScore.AddScore();
        scoreText.text = teamScore.ToString();
        randomizer.NextOrder(order);
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

}
