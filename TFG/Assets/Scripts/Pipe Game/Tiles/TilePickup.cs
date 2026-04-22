using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Permite al jugador coger una loseta del suelo al interaccionar con ella.
/// Funciona con cualquier ICarryObject en el mismo GameObject (PipeTile, SimpleCarryable...).
/// </summary>
public class TilePickup : InteractableObject
{
    private ICarryObject carryable;

    private void Awake()
    {
        // Buscar cualquier componente que implemente ICarryObject (PipeTile tiene prioridad)
        carryable = GetComponent<PipeTile>();
        if (carryable == null)
            carryable = GetComponent<SimpleCarryable>();
    }

    protected override void InteractOffline(GameObject player)
    {
        if (carryable == null) return;
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
        if (carryable == null) return;
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();
        if (carry == null || carry.isCarrying) return;
        carry.CarryObject(carryable);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        // Icono de loseta disponible
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.6f);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.05f,
            new Vector3(0.8f, 0.08f, 0.8f));
    }
}
