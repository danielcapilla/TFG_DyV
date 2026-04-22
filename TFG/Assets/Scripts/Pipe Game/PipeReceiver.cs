using Unity.Netcode;
using UnityEngine;
using System;

/// <summary>
/// Punto de llegada del circuito de tuberias.
/// Dispara OnCircuitCompleted cuando recibe la señal del generador.
/// El feedback visual se gestiona via gizmos y eventos — no requiere un Renderer.
/// </summary>
public class PipeReceiver : NetworkBehaviour
{
    [Header("Direccion de entrada")]
    [SerializeField] private TileDirection inputDirection = TileDirection.South;

    public static event Action OnCircuitCompleted;
    public static event Action OnCircuitBroken;

    private NetworkVariable<bool> isConnected = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public TileDirection InputDirection => inputDirection;
    public bool IsConnected => isConnected.Value;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isConnected.OnValueChanged += HandleConnectionChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isConnected.OnValueChanged -= HandleConnectionChanged;
    }

    private void HandleConnectionChanged(bool prev, bool next)
    {
        OnConnectionChanged(next);
        if (next && !prev) NotifyCompletedClientRpc();
        if (!next && prev) NotifyBrokenClientRpc();
    }

    /// <summary>Llamado por PipeConnectionChecker.</summary>
    public void SetConnected(bool connected, bool offline = false)
    {
        if (!offline && !IsServer) return;
        if (offline)
        {
            OnConnectionChanged(connected);
            if (connected) OnCircuitCompleted?.Invoke();
            else OnCircuitBroken?.Invoke();
            return;
        }
        isConnected.Value = connected;
    }

    /// <summary>Sobrescribir para reaccionar visualmente al cambio de estado.</summary>
    protected virtual void OnConnectionChanged(bool connected) { }

    [ClientRpc] private void NotifyCompletedClientRpc() => OnCircuitCompleted?.Invoke();
    [ClientRpc] private void NotifyBrokenClientRpc()    => OnCircuitBroken?.Invoke();

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        Gizmos.color = IsConnected ? Color.green : Color.cyan;
        Vector3 dir = DirectionToVector(inputDirection);
        Gizmos.DrawLine(transform.position, transform.position + dir * 0.6f);
        Gizmos.DrawSphere(transform.position + dir * 0.6f, 0.1f);
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f,
            $"REC ← {inputDirection}");
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
