using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PipeTile : NetworkBehaviour, ICarryObject, IRotatableObject
{
    [Header("Forma")]
    [SerializeField] private TileShapeType shapeType = TileShapeType.Straight;

    [Header("Bloqueo")]
    [SerializeField] private Material lockedMaterial;
    private Material originalMaterial;
    private Renderer tileRenderer;
    public bool IsLocked { get; private set; }

    // Rotacion en online (sincronizada)
    private NetworkVariable<int> rotationDegrees = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Rotacion en offline (local)
    private int offlineRotation = 0;

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    private void Awake()
    {
        tileRenderer = GetComponentInChildren<Renderer>();
        if (tileRenderer != null) originalMaterial = tileRenderer.sharedMaterial;
    }
    private bool isBeingCarried = false;
    public TileSlot CurrentSlot { get; private set; }
    public void SetCurrentSlot(TileSlot slot) => CurrentSlot = slot;

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
        if (tileRenderer == null) return;
        if (locked && lockedMaterial != null)
            tileRenderer.material = lockedMaterial;
        else if (!locked && originalMaterial != null)
            tileRenderer.material = originalMaterial;
    }

    // ── ICarryObject ──────────────────────────────────────────────────────────
    public string CarryType => "tile";
    public bool CanBePickedUp => true;
    public NetworkObject GetNetworkObject() => NetworkObject;
    public GameObject GetGameObject() => gameObject;
    public void OnPickedUp(PlayerCarry carrier)
    {
        isBeingCarried = true;
        // Al coger, asegurarse de que el slot anterior queda limpio
        if (CurrentSlot != null)
        {
            CurrentSlot.Clear();
            CurrentSlot = null;
        }
    }
    public void OnDelivered(ICarryReceiver receiver) { isBeingCarried = false; }
    public void OnRejected(ICarryReceiver receiver) { isBeingCarried = false; }

    // ── Aperturas ─────────────────────────────────────────────────────────────

    public TileShapeType ShapeType => shapeType;

    public int CurrentRotation => IsOffline ? offlineRotation : rotationDegrees.Value;

    private void LateUpdate()
    {
        // Mientras es transportado, mantener la rotacion mundial alineada
        // al multiplo de 90 almacenado, ignorando la rotacion del jugador
        if (!isBeingCarried) return;
        transform.rotation = Quaternion.Euler(0, CurrentRotation, 0);
    }

    public TileDirection Openings =>
        TileShapeData.Rotate(TileShapeData.GetOpenings(shapeType), CurrentRotation);

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

    private void ApplyRotationVisual(int deg)
    {
        transform.localRotation = Quaternion.Euler(0, deg, 0);
    }

    public void Rotate90()
    {
        // Si no esta spawneado (instanciado por el filler antes de que Netcode lo registre)
        // o estamos en offline, usar siempre el campo local
        if (IsOffline || !NetworkObject.IsSpawned)
        {
            offlineRotation = (offlineRotation + 90) % 360;
            ApplyRotationVisual(offlineRotation);
        }
        else if (IsServer)
        {
            rotationDegrees.Value = (rotationDegrees.Value + 90) % 360;
        }
        else
        {
            RotateServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RotateServerRpc() =>
        rotationDegrees.Value = (rotationDegrees.Value + 90) % 360;

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
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
