using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class DeliveryStation : InteractableObject
{
    [SerializeField] RecipeRandomizer randomizer;
    [SerializeField] Transform endPos;
    [SerializeField] float time;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);

        //Si lleva un plato
        if (player.carryingObject.GetGameObject().TryGetComponent<PlateBehaviour>(out PlateBehaviour plate)) 
        {
            //Colocar plato en superficie
            //Animar plato (con DOTween puede que con alguna corutina o algo) para que sea entregado
            plate.transform.DOMove(endPos.position, time).SetEase(Ease.InQuart);
            //Evaluar plato respecto al pedido
            bool same = true;
            if (randomizer.currentOrder.Count == plate.Ingredients.Count)
            {
                for (int i = 0; i < randomizer.currentOrder.Count; ++i)
                {
                    if (randomizer.currentOrder[i] != plate.Ingredients[i])
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
            if (same) 
            {
                //TODO añadir puntuacion
            }
        }
    }

    [ServerRpc]
    public void DeliverPlateServerRPC() 
    {
        
    }

    [ClientRpc]
    public void MovePlateClientRPC() 
    {
        
    }
}
