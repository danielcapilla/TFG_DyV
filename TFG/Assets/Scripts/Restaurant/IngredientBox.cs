using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IngredientBox : InteractableObject
{
    public IngredientsScriptableObject ingredient;


    private void Start()
    {
        GameObject instance = Instantiate(ingredient.Model,transform);
        instance.transform.localPosition = new Vector3(0,0.5f,0);
    }

    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);
        SpawnMultiplayerServerRPC(player.GetNetworkObject());
        
    }
    [ServerRpc(RequireOwnership = false)]
    void SpawnMultiplayerServerRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerCarry playerCarry = playerNetworkObject.GetComponent<PlayerCarry>();
        if (!playerCarry.isCarrying)
        {
            GameObject instance = Instantiate(ingredient.Model);

            //Para que lo vean los clientes
            NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn(true);

            IngredientBehaviour carryObject = instance.AddComponent<IngredientBehaviour>();
            //Poner de padre al player que lo ha invocado (solo funciona si se pone en una serverRPC)
            carryObject.transform.SetParent(playerCarry.transform);
            carryObject.ingredient = ingredient;
            playerCarry.CarryObject(carryObject);
        }
    }
    //Missing box display its content and highlight when player can interact
}
