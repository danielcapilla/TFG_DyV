using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateSpawner : InteractableObject
{
    [SerializeField] GameObject plate;
    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);
        SpawnMultiplayerServerRPC(player.GetNetworkObject());
    
    }
    [ServerRpc (RequireOwnership = false)]
    private void SpawnMultiplayerServerRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerCarry playerCarry = playerNetworkObject.GetComponent<PlayerCarry>();
        if (!playerCarry.isCarrying)
        {
            GameObject instance = Instantiate(plate);
            ICarryObject carryObject = instance.GetComponent<PlateBehaviour>();
            //Para que lo vean los clientes
            NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn(true);


            //Poner de padre al player que lo ha invocado (solo funciona si se pone en una serverRPC)
            carryObject.GetGameObject().transform.SetParent(playerCarry.transform);
            playerCarry.CarryObject(carryObject);
        }
    }
}
