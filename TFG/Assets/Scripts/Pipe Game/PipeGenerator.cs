using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Punto de inicio del circuito de tuberias.
/// Emite una señal cuando hay un camino completo hasta el PipeReceiver.
/// Coloca este GameObject en un TileSlot especial o en el borde de la cuadricula.
/// </summary>
public class PipeGenerator : NetworkBehaviour
{
    [Header("Direccion de salida")]
    [SerializeField] private TileDirection outputDirection = TileDirection.North;

    // ── Estado ────────────────────────────────────────────────────────────────

    private NetworkVariable<bool> isConnected = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public TileDirection OutputDirection => outputDirection;
    public bool IsConnected => isConnected.Value;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isConnected.OnValueChanged += (_, val) => OnConnectionChanged(val);
    }

    /// <summary>Llamado por PipeConnectionChecker cuando evalua el circuito.</summary>
    public void SetConnected(bool connected, bool offline = false)
    {
        if (!offline && !IsServer) return;
        if (offline)
        {
            OnConnectionChanged(connected);
            return;
        }
        isConnected.Value = connected;
    }

    // Sobrescribir para reaccionar al cambio (p.ej. activar una luz, un efecto, etc.)
    protected virtual void OnConnectionChanged(bool connected) { }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsConnected ? Color.green : Color.red;
        Vector3 dir = DirectionToVector(outputDirection);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + dir * 0.6f);
        Gizmos.DrawSphere(transform.position + Vector3.up + dir * 0.6f, 0.1f);
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f,
            $"GEN → {outputDirection}");
    }
#endif

    private Vector3 DirectionToVector(TileDirection d)
    {
        switch (d)
        {
            case TileDirection.North: return Vector3.forward;
            case TileDirection.South: return -Vector3.forward;
            case TileDirection.East:  return Vector3.right;
            case TileDirection.West:  return -Vector3.right;
            default: return Vector3.zero;
        }
    }
}
