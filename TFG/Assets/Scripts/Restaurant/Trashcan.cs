using Unity.Netcode;
using UnityEngine;

public class Trashcan : InteractableObject
{
    protected override void InteractOffline(GameObject player)
    {
        PlayerCarry carry = player.GetComponent<PlayerCarry>();
        if (carry == null || !carry.isCarrying) return;
        ICarryObject toDestroy = carry.DropObject();
        if (toDestroy == null) return;
        foreach (ICarryObject obj in toDestroy.GetGameObject().transform.GetComponentsInChildren<ICarryObject>())
            Destroy(obj.GetGameObject());
        Destroy(toDestroy.GetGameObject());
    }

    protected override void InteractOnline(GameObject player)
    {
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj != null) DespawnServerRPC(netObj);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();
        if (carry == null || !carry.isCarrying) return;
        ICarryObject toDestroy = carry.DropObject();
        if (toDestroy == null) return;
        foreach (ICarryObject obj in toDestroy.GetGameObject().transform.GetComponentsInChildren<ICarryObject>())
            obj.GetNetworkObject().Despawn(obj.GetGameObject());
    }
}
