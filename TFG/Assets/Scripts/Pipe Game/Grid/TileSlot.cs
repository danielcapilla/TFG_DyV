using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Hueco de la cuadricula donde el jugador puede dejar una loseta (PipeTile).
/// Busca automaticamente el PipeConnectionChecker en la escena al inicializar.
/// </summary>
public class TileSlot : InteractableObject, ICarryReceiver
{
    private bool isOccupied = false;
    private ICarryObject placedTile = null;

    private PipeConnectionChecker connectionChecker;

    [SerializeField] private Vector3 tileOffset = new Vector3(0, 0.05f, 0);

    /// <summary>La PipeTile colocada en este slot, o null si esta vacio.</summary>
    public PipeTile PlacedTile => placedTile?.GetGameObject()?.GetComponent<PipeTile>();

    private void Awake()
    {
        connectionChecker = FindFirstObjectByType<PipeConnectionChecker>();
    }

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
        // Leer la rotacion del PipeTile (multiplo de 90) e ignorar la rotacion del jugador
        PipeTile pipeTile = carryObject.GetGameObject().GetComponent<PipeTile>();
        int snapDeg = pipeTile != null ? pipeTile.CurrentRotation : 0;
        PlayerCarry.SetParentSafe(carryObject.GetGameObject(), transform);
        carryObject.GetGameObject().transform.localPosition = tileOffset;
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
            PlayerCarry.SetParentSafe(placedTile.GetGameObject(), null);
            carry.TryPickUp(placedTile);
            placedTile = null;
            isOccupied = false;
            connectionChecker?.EvaluateCircuit();
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

        PipeTile pipeTile = tileNet.GetComponent<PipeTile>();
        int snapDeg = pipeTile != null ? pipeTile.CurrentRotation : 0;
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
        PipeTile tile = PlacedTile;
        if (tile == null) return;

        // Dibujar las aperturas de la loseta colocada
        DrawOpeningGizmos(tile.Openings, transform.position + Vector3.up * 0.1f, 0.35f);
    }

    private void OnDrawGizmosSelected()
    {
        // Mostrar el slot aunque este vacio al seleccionarlo
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
