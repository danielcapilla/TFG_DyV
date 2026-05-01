using Unity.Netcode;
using UnityEngine;

public class TileSlot : InteractableObject, ICarryReceiver
{
    private bool isOccupied = false;
    private ICarryObject placedTile = null;

    private PipeConnectionChecker connectionChecker;

    [SerializeField] private Vector3 tileOffset = new Vector3(0f, 0.05f, 0f);

    public PipeTile PlacedTile => placedTile?.GetGameObject()?.GetComponent<PipeTile>();

    private void Awake()
    {
        connectionChecker = FindFirstObjectByType<PipeConnectionChecker>();
    }


    public override bool CanInteract(GameObject player)
    {
        PlayerCarry carry = player.GetComponent<PlayerCarry>();
        if (carry == null) return false;
        // Tile bloqueada: no se puede coger
        if (isOccupied && PlacedTile != null && PlacedTile.IsLocked) return false;
        // Interactuable si: el slot tiene tile (para cogerla) O el jugador lleva tile (para colocarla)
        return isOccupied || (carry.isCarrying && carry.carryingObject?.CarryType == "tile");
    }

    // ── ForcePlace (usado por PipeGridFiller) ─────────────────────────────────

    public void ForcePlace(ICarryObject carry)
    {
        placedTile = carry;
        isOccupied = true;
        // Registrar este slot en la tile para relacion bidireccional
        carry.GetGameObject()?.GetComponent<PipeTile>()?.SetCurrentSlot(this);
    }

    public void Clear()
    {
        if (placedTile != null)
            placedTile.GetGameObject()?.GetComponent<PipeTile>()?.SetCurrentSlot(null);
        placedTile = null;
        isOccupied = false;
    }

    // ── ICarryReceiver ────────────────────────────────────────────────────────

    public bool CanReceive(ICarryObject carryObject, PlayerCarry carrier)
        => !isOccupied && carryObject != null;

    public bool Receive(ICarryObject carryObject, PlayerCarry carrier)
    {
        if (!CanReceive(carryObject, carrier)) return false;
        // Limpiar slot anterior si la tile venia de otro slot
        var pipeTile = carryObject.GetGameObject()?.GetComponent<PipeTile>();
        pipeTile?.CurrentSlot?.Clear();
        placedTile = carryObject;
        isOccupied = true;
        pipeTile?.SetCurrentSlot(this);
        PlayerCarry.SetParentSafe(carryObject.GetGameObject(), transform);
        carryObject.GetGameObject().transform.localPosition = tileOffset;
        PipeTile pt = carryObject.GetGameObject().GetComponent<PipeTile>();
        int snapDeg = pt != null ? pt.CurrentRotation : 0;
        carryObject.GetGameObject().transform.localRotation = Quaternion.Euler(0, snapDeg, 0);
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
            var tile = placedTile;
            Clear();
            PlayerCarry.SetParentSafe(tile.GetGameObject(), null);
            carry.TryPickUp(tile);
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
            NetworkObject tileNet = carry.carryingObject.GetNetworkObject();
            tileNet.TrySetParent((NetworkObject)null);
            PlaceTileClientRPC(playerRef, tileNet);
        }
        else if (isOccupied && !carry.isCarrying)
        {
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
        carry.DropObject();
        PipeTile pt = tileNet.GetComponent<PipeTile>();
        int snapDeg = pt != null ? pt.CurrentRotation : 0;
        PlayerCarry.SetParentSafe(tileNet.gameObject, transform);
        tileNet.transform.localPosition = tileOffset;
        tileNet.transform.localRotation = Quaternion.Euler(0, snapDeg, 0);
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
        carry.SetCarryingObject(tile);
        PlayerCarry.SetParentSafe(tileNet.gameObject, carry.GetCarryPosition());
        tileNet.transform.localPosition = Vector3.zero;
        tileNet.transform.localRotation = Quaternion.identity;
        tile.OnPickedUp(carry);
        placedTile = null;
        isOccupied = false;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (placedTile == null) return;
        // Comprobar que el objeto no fue destruido (MissingReferenceException)
        var go = placedTile.GetGameObject();
        if (go == null) { placedTile = null; return; }
        PipeTile tile = go.GetComponent<PipeTile>();
        if (tile == null) return;
        DrawOpeningGizmos(tile.Openings, transform.position + Vector3.up * 0.1f, 0.35f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position + Vector3.up * 0.05f, new Vector3(0.9f, 0.05f, 0.9f));
    }

    internal static void DrawOpeningGizmos(TileDirection openings, Vector3 center, float size)
    {
        if ((openings & TileDirection.North) != 0) DrawArrow(center, Vector3.forward,  Color.blue, size);
        if ((openings & TileDirection.South) != 0) DrawArrow(center, -Vector3.forward, Color.blue, size);
        if ((openings & TileDirection.East)  != 0) DrawArrow(center, Vector3.right,    Color.blue, size);
        if ((openings & TileDirection.West)  != 0) DrawArrow(center, -Vector3.right,   Color.blue, size);
    }

    private static void DrawArrow(Vector3 origin, Vector3 dir, Color color, float len)
    {
        Gizmos.color = color;
        Vector3 end = origin + dir * len;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawSphere(end, 0.05f);
    }
}
