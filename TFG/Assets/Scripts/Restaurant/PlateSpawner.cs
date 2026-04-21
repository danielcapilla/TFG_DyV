using Unity.Netcode;
using UnityEngine;

public class PlateSpawner : InteractableObject
{
    [SerializeField] GameObject plate;

    protected override void InteractOnline(GameObject player)
    {
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj != null) SpawnServerRPC(netObj);
    }

    protected override void InteractOffline(GameObject player)
    {
        PlayerCarry carry = player.GetComponent<PlayerCarry>();
        if (carry == null || carry.isCarrying) return;
        GameObject instance = Instantiate(plate);
        carry.TryPickUp(instance.GetComponent<PlateBehaviour>());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();
        if (carry == null || carry.isCarrying) return;
        GameObject instance = Instantiate(plate);
        NetworkObject netObj = instance.GetComponent<NetworkObject>();
        netObj.Spawn(true);
        carry.CarryObject(instance.GetComponent<PlateBehaviour>());
    }
}
