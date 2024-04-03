using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Trashcan : InteractableObject
{
    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);
        DespawnMultiplayerServerRPC(player.GetNetworkObject());
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void DespawnMultiplayerServerRPC(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerCarry playerCarry = playerNetworkObject.GetComponent<PlayerCarry>();

        if (playerCarry.isCarrying)
        {
            ICarryObject destroyObject = playerCarry.DropObject();
            if (destroyObject != null)
            {
                destroyObject.GetNetworkObject().Despawn(destroyObject.GetGameObject());
            }
        }
    }
}
