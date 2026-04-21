using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Permite al jugador coger la baldosa del suelo al interaccionar con ella.
/// Requiere SimpleCarryable en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(SimpleCarryable))]
public class TilePickup : InteractableObject
{
    private SimpleCarryable carryable;

    private void Awake()
    {
        carryable = GetComponent<SimpleCarryable>();
    }

    protected override void InteractOffline(GameObject player)
    {
        PlayerCarry carry = player.GetComponent<PlayerCarry>();
        if (carry == null || carry.isCarrying) return;
        carry.TryPickUp(carryable);
    }

    protected override void InteractOnline(GameObject player)
    {
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj != null) PickUpServerRPC(netObj);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PickUpServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();
        if (carry == null || carry.isCarrying) return;
        carry.CarryObject(carryable);
    }
}
