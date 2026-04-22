using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Loseta de tuberia con forma configurable.
/// Implementa ICarryObject para poder cogerse y colocarse en TileSlots.
/// TilePickup en el mismo GameObject permite cogerla del suelo.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class PipeTile : NetworkBehaviour, ICarryObject
{
    [Header("Forma")]
    [SerializeField] private TileShapeType shapeType = TileShapeType.Straight;

    private NetworkVariable<int> rotationDegrees = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // ── ICarryObject ──────────────────────────────────────────────────────────
    public string CarryType => "tile";
    public bool CanBePickedUp => true;
    public NetworkObject GetNetworkObject() => NetworkObject;
    public GameObject GetGameObject() => gameObject;
    public void OnPickedUp(PlayerCarry carrier) { }
    public void OnDelivered(ICarryReceiver receiver) { }
    public void OnRejected(ICarryReceiver receiver) { }

    // ── Aperturas ─────────────────────────────────────────────────────────────

    public TileShapeType ShapeType => shapeType;

    public TileDirection Openings =>
        TileShapeData.Rotate(TileShapeData.GetOpenings(shapeType), rotationDegrees.Value);

    public bool HasOpening(TileDirection dir) => (Openings & dir) != 0;

    // ── Rotacion ──────────────────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rotationDegrees.OnValueChanged += OnRotationChanged;
        ApplyRotationVisual(rotationDegrees.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        rotationDegrees.OnValueChanged -= OnRotationChanged;
    }

    private void OnRotationChanged(int prev, int next) => ApplyRotationVisual(next);

    private void ApplyRotationVisual(int deg) =>
        transform.localRotation = Quaternion.Euler(0, deg, 0);

    public void Rotate90()
    {
        int next = (rotationDegrees.Value + 90) % 360;
        if (IsServer || !NetworkObject.IsSpawned)
            rotationDegrees.Value = next;
        else
            RotateServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void RotateServerRpc() =>
        rotationDegrees.Value = (rotationDegrees.Value + 90) % 360;

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        // Calcular aperturas con la rotacion actual del transform (en editor puede no estar sincronizado)
        int deg = Mathf.RoundToInt(transform.eulerAngles.y / 90f) * 90;
        TileDirection openings = TileShapeData.Rotate(TileShapeData.GetOpenings(shapeType), deg);
        TileSlot.DrawOpeningGizmos(openings, transform.position + Vector3.up * 0.15f, 0.4f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            $"{shapeType}\n{Mathf.RoundToInt(transform.eulerAngles.y)}°");
    }
#endif
}
