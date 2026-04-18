using Unity.Netcode;
using UnityEngine;

public class PlateSpawner : InteractableObject
{
    [SerializeField] GameObject plate;

    protected override void InteractOnline(PlayerCarry player) => SpawnServerRPC(player.GetNetworkObject());

    protected override void InteractOffline(PlayerCarry player)
    {
        if (player.isCarrying) return;
        GameObject instance = Instantiate(plate);
        player.TryPickUp(instance.GetComponent<PlateBehaviour>());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry playerCarry = playerNet.GetComponent<PlayerCarry>();
        if (playerCarry.isCarrying) return;

        GameObject instance = Instantiate(plate);
        NetworkObject netObj = instance.GetComponent<NetworkObject>();
        netObj.Spawn(true);
        playerCarry.CarryObject(instance.GetComponent<PlateBehaviour>());
    }
}
