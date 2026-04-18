using Unity.Netcode;
using UnityEngine;

public class Trashcan : InteractableObject
{
    protected override void InteractOffline(PlayerCarry player)
    {
        if (!player.isCarrying) return;

        ICarryObject toDestroy = player.DropObject();
        if (toDestroy == null) return;

        // En offline destruimos los objetos normalmente
        foreach (ICarryObject obj in toDestroy.GetGameObject()
                     .transform.GetComponentsInChildren<ICarryObject>())
        {
            Destroy(obj.GetGameObject());
        }
        Destroy(toDestroy.GetGameObject());
    }

    protected override void InteractOnline(PlayerCarry player)
    {
        DespawnServerRPC(player.GetNetworkObject());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry playerCarry = playerNet.GetComponent<PlayerCarry>();
        if (!playerCarry.isCarrying) return;

        ICarryObject toDestroy = playerCarry.DropObject();
        if (toDestroy == null) return;

        foreach (ICarryObject obj in toDestroy.GetGameObject()
                     .transform.GetComponentsInChildren<ICarryObject>())
        {
            obj.GetNetworkObject().Despawn(obj.GetGameObject());
        }
    }
}
