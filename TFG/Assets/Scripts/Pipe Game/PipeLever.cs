using UnityEngine;

/// <summary>
/// Palanca interactuable que evalua el circuito de tuberias al accionarse.
/// Anyadir este componente a un GameObject con un Collider.
/// </summary>
public class PipeLever : InteractableObject
{
    [Header("Referencias")]
    [SerializeField] private PipeConnectionChecker connectionChecker;

    [Header("Animacion")]
    [SerializeField] private Animator animator;
    private static readonly int PullHash = Animator.StringToHash("pull");

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 1f;
    private float lastUsed = -999f;

    // ── Offline ───────────────────────────────────────────────────────────────

    protected override void InteractOffline(GameObject player)
    {
        TryActivate();
    }

    // ── Online ────────────────────────────────────────────────────────────────

    protected override void InteractOnline(GameObject player)
    {
        // Cualquier cliente puede accionar la palanca — el checker filtra si !IsServer
        TryActivate();
    }

    // ── Logica ────────────────────────────────────────────────────────────────

    private void TryActivate()
    {
        if (Time.time - lastUsed < cooldown) return;
        lastUsed = Time.time;

        if (animator != null)
            animator.SetTrigger(PullHash);

        Debug.Log("Evaluando");
        connectionChecker?.EvaluateCircuit();
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.3f, new Vector3(0.3f, 0.6f, 0.3f));
        Gizmos.DrawLine(transform.position + Vector3.up * 0.6f,
                        transform.position + Vector3.up * 0.6f + transform.forward * 0.3f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = new Color(1f, 0.5f, 0f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.9f, "LEVER");
    }
#endif
}
