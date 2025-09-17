using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    [Header("Variables de movimiento")]
    public float moveDistance = 1f; // Distancia por movimiento
    public float moveDuration = 0.5f; // Duracion del movimiento
    public float rotationSpeed = 10f;
    public bool IsMoving { get; private set; }

    private NetworkVariable<Vector3> targetPosition = new NetworkVariable<Vector3>( Vector3.zero,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Coroutine currentMovementCoroutine;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            targetPosition.Value = transform.position;
        }

        targetPosition.OnValueChanged += OnTargetPositionChanged;
    }

    private void OnTargetPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        // Solo iniciar movimiento si no estamos ya moviendonos
        if (!IsMoving && currentMovementCoroutine == null)
        {
            currentMovementCoroutine = StartCoroutine(MoveToPositionCoroutine(newValue));
        }
    }

    private IEnumerator MoveToPositionCoroutine(Vector3 targetPos)
    {
        IsMoving = true;
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        IsMoving = false;
        currentMovementCoroutine = null;
    }

    // Metodos llamados por los comandos (solo en servidor)
    public void MoveLeft()
    {
        if (!IsServer || IsMoving) return;
        targetPosition.Value += Vector3.back * moveDistance;
    }

    public void MoveRight()
    {
        if (!IsServer || IsMoving) return;
        targetPosition.Value += Vector3.forward * moveDistance;
    }

    public void MoveUp()
    {
        if (!IsServer || IsMoving) return;
        targetPosition.Value += Vector3.left * moveDistance;
    }

    public void MoveDown()
    {
        if (!IsServer || IsMoving) return;
        targetPosition.Value += Vector3.right * moveDistance;
    }

    public void StopMovement()
    {
        if (!IsServer) return;
        targetPosition.Value = transform.position;
    }

    public override void OnNetworkDespawn()
    {
        targetPosition.OnValueChanged -= OnTargetPositionChanged;
    }
}