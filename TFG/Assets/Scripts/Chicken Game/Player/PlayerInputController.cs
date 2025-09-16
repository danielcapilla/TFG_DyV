using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    [Header("Variables de movimiento")]
    public float moveDistance = 1f; // Distancia por movimiento
    public float moveDuration = 0.5f; // Duración del movimiento
    public float rotationSpeed = 10f;

    private bool isMoving = false;
    private Rigidbody rb;
    private NetworkVariable<Vector3> targetPosition = new NetworkVariable<Vector3>( Vector3.zero,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        targetPosition.OnValueChanged += OnTargetPositionChanged;

        if (IsServer)
        {
            targetPosition.Value = transform.position;
        }
    }

    private void OnTargetPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        // Iniciar movimiento suave hacia la nueva posicion
        if (!isMoving)
        {
            StartCoroutine(MoveToPosition(newValue));
        }
    }

    // Movimiento por pasos (solo servidor puede llamar)
    public void MoveLeft()
    {
        if (!IsServer || isMoving) return;
        targetPosition.Value += Vector3.left * moveDistance;
    }

    public void MoveRight()
    {
        if (!IsServer || isMoving) return;
        targetPosition.Value += Vector3.right * moveDistance;
    }

    public void MoveUp()
    {
        if (!IsServer || isMoving) return;
        targetPosition.Value += Vector3.forward * moveDistance;
    }

    public void MoveDown()
    {
        if (!IsServer || isMoving) return;
        targetPosition.Value += Vector3.back * moveDistance;
    }

    public void StopMovement()
    {
        if (!IsServer) return;
        targetPosition.Value = transform.position;
    }

    // Corrutina para movimiento suave
    private System.Collections.IEnumerator MoveToPosition(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos; // Asegurar posicion exacta
        isMoving = false;
    }

    public override void OnNetworkDespawn()
    {
        targetPosition.OnValueChanged -= OnTargetPositionChanged;
    }
}