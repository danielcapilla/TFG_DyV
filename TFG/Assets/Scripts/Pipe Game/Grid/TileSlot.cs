using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Hueco de la cuadricula donde el jugador puede dejar una baldosa.
/// </summary>
public class TileSlot : InteractableObject, ICarryReceiver
{
    private bool isOccupied = false;
    private ICarryObject placedTile = null;

    [SerializeField] private Vector3 tileOffset = new Vector3(0, 0.05f, 0);

    // ── ICarryReceiver ────────────────────────────────────────────────────────

    public bool CanReceive(ICarryObject carryObject, PlayerCarry carrier)
    {
        return !isOccupied && carryObject != null;
    }

    public bool Receive(ICarryObject carryObject, PlayerCarry carrier)
    {
        if (!CanReceive(carryObject, carrier)) return false;
        placedTile = carryObject;
        isOccupied = true;
        PlayerCarry.SetParentSafe(carryObject.GetGameObject(), transform);
        carryObject.GetGameObject().transform.localPosition = tileOffset;
        carryObject.GetGameObject().transform.localRotation = Quaternion.identity;
        return true;
    }

    // ── Offline ───────────────────────────────────────────────────────────────

    protected override void InteractOffline(GameObject player)
    {
        PlayerCarry carry = player.GetComponent<PlayerCarry>();
        if (carry == null) return;

        if (!isOccupied && carry.isCarrying)
        {
            ICarryObject tile = carry.DropObject();
            if (tile != null) Receive(tile, carry);
        }
        else if (isOccupied && !carry.isCarrying)
        {
            PlayerCarry.SetParentSafe(placedTile.GetGameObject(), null);
            carry.TryPickUp(placedTile);
            placedTile = null;
            isOccupied = false;
        }
    }

    // ── Online ────────────────────────────────────────────────────────────────

    protected override void InteractOnline(GameObject player)
    {
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj != null) InteractServerRPC(netObj);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void InteractServerRPC(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();
        if (carry == null) return;

        if (!isOccupied && carry.isCarrying)
        {
            // Actualizar estado Netcode del objeto (desenparentar del jugador)
            NetworkObject tileNet = carry.carryingObject.GetNetworkObject();
            tileNet.TrySetParent((NetworkObject)null);
            PlaceTileClientRPC(playerRef, tileNet);
        }
        else if (isOccupied && !carry.isCarrying)
        {
            // Actualizar estado Netcode (hacer hijo del jugador)
            NetworkObject tileNet = placedTile.GetNetworkObject();
            tileNet.TrySetParent(playerNet);
            PickUpFromSlotClientRPC(playerRef, tileNet);
        }
    }

    [ClientRpc]
    private void PlaceTileClientRPC(NetworkObjectReference playerRef, NetworkObjectReference tileRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        if (!tileRef.TryGet(out NetworkObject tileNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();

        // Soltar estado local del jugador
        ICarryObject tile = carry.DropObject();

        // Colocar visualmente en el hueco en cada cliente
        PlayerCarry.SetParentSafe(tileNet.gameObject, transform);
        tileNet.transform.localPosition = tileOffset;
        tileNet.transform.localRotation = Quaternion.identity;

        placedTile = tileNet.GetComponent<ICarryObject>();
        isOccupied = true;
    }

    [ClientRpc]
    private void PickUpFromSlotClientRPC(NetworkObjectReference playerRef, NetworkObjectReference tileRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNet)) return;
        if (!tileRef.TryGet(out NetworkObject tileNet)) return;
        PlayerCarry carry = playerNet.GetComponent<PlayerCarry>();
        ICarryObject tile = tileNet.GetComponent<ICarryObject>();
        if (tile == null) return;

        // Coger visualmente en cada cliente
        carry.SetCarryingObject(tile);
        PlayerCarry.SetParentSafe(tileNet.gameObject, carry.GetCarryPosition());
        tileNet.transform.localPosition = Vector3.zero;
        tileNet.transform.localRotation = Quaternion.identity;
        tile.OnPickedUp(carry);

        placedTile = null;
        isOccupied = false;
    }
}
