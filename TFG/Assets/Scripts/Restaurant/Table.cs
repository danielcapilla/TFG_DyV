using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Table : InteractableObject
{
    bool isOccupied = false;
    [SerializeField] Transform placePosition;
    ICarryObject holdingObject;

    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);
        ReplaceObjectsServerRPC(player.GetNetworkObject());
        
    }
    [ServerRpc (RequireOwnership = false)]
    private void ReplaceObjectsServerRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerCarry playerCarry = playerNetworkObject.GetComponent<PlayerCarry>();

        if (!isOccupied && playerCarry.isCarrying)
        {
            ReplaceObjectsClientRPC(playerNetworkObjectReference);          
        }
        else if (isOccupied && !playerCarry.isCarrying)
        {
            holdingObject.GetGameObject().transform.parent = playerCarry.transform;
            playerCarry.CarryObject(holdingObject);
            isOccupied = false;
        }
        //TODO if plate on table and player holding ingredient, add ingredient
        else if (isOccupied && playerCarry.isCarrying)
        {
            if (holdingObject.GetGameObject().TryGetComponent<PlateBehaviour>(out PlateBehaviour plate))
            {
                if (playerCarry.carryingObject.GetGameObject().TryGetComponent<IngredientBehaviour>(out IngredientBehaviour ingredient))
                {
                    plate.AddIngredient((IngredientBehaviour)playerCarry.DropObject());
                }
            }
        }
    }
    [ClientRpc]
    private void ReplaceObjectsClientRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerCarry playerCarry = playerNetworkObject.GetComponent<PlayerCarry>();
        holdingObject = playerCarry.DropObject();
        SetParentServerRPC();
        Debug.Log(holdingObject);

        holdingObject.GetGameObject().transform.localPosition = placePosition.localPosition;
        isOccupied = true;
    }
    [ServerRpc (RequireOwnership = false)]
    private void SetParentServerRPC()
    {
        holdingObject.GetGameObject().transform.parent = this.transform;
    }
}
