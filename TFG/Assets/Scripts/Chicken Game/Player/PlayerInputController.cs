using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    [Header("Variables de movimiento")]
    public float moveDistance = 1f; // Distancia por movimiento
    public float moveDuration = 0.5f; // Duracion del movimiento
    public float rotationSpeed = 10f;
    public Vector2Int CurrentGridPos { get; private set; }
    public bool IsMoving { get; private set; }
    public bool LastMoveBlocked { get; private set; }
    [SerializeField] private LayerMask obstacleMask;
    [Header("Partículas")]
    private ParticleSystem dust;
    [Header("Localizadores")]
    [SerializeField] private Light light;
    [SerializeField] private GameObject halo;
    private GameManagerChicken gameManagerChicken;
    public NetworkVariable<Vector3> targetPosition = new NetworkVariable<Vector3>( Vector3.zero,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Coroutine currentMovementCoroutine;
    
    public Action<int> OnObstaculeCollided;

    [Header("Modo tutorial")]
    public bool isTutorialMode = false;

    private void Start()
    {
        dust = GetComponentInChildren<ParticleSystem>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            targetPosition.Value = transform.position;
            gameManagerChicken = FindFirstObjectByType<GameManagerChicken>();
            if(gameManagerChicken != null)
                gameManagerChicken.OnPlayerSpawned += ActivateIdentificators;
            UpdateCurrentGridPos(transform.position);

        }
        targetPosition.OnValueChanged += OnTargetPositionChanged;
    }
    private void Update()
    {
        if (!light.gameObject.activeSelf) return;
        float t = (Mathf.Sin(Time.time * 5f) + 1f) / 2f;
        light.intensity = Mathf.Lerp(0f, 2f, t);
    }
    private void ActivateIdentificators(NetworkObjectReference playerNOR, int idGroup)
    {
        TeamInfo teamInfo = TeamMenager.Instance.teams[GetComponentInParent<PlayerStats>().idGrupo.Value];
        ulong[] targetClients = teamInfo.integrantes.ToArray();
        ActivateIdentificatorsForClientRPC(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClients
            }
        });
    }

    [ClientRpc]
    private void ActivateIdentificatorsForClientRPC(ClientRpcParams clientRpcParams)
    {
        light.gameObject.SetActive(true);
        halo.SetActive(true);
    }

    private void OnTargetPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        // Actualizar la pos
        UpdateCurrentGridPos(newValue);

        // Solo iniciar movimiento si no estamos ya moviendonos
        if (!IsMoving && currentMovementCoroutine == null)
        {
            currentMovementCoroutine = StartCoroutine(MoveToPositionCoroutine(newValue));
        }
    }

    private IEnumerator MoveToPositionCoroutine(Vector3 targetPos)
    {
        IsMoving = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 startPos = rb.position;
        float elapsedTime = 0f;

        Vector3 direction = (targetPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, targetPos);

        LastMoveBlocked = false;
        // Comprobar colisiones con raycast
        if (Physics.Raycast(startPos, direction, distance, obstacleMask))
        {
            LastMoveBlocked = true;                          
            targetPosition.Value = transform.position;
            IsMoving = false;
            currentMovementCoroutine = null;
            //gameManagerChicken.PlayCollisionSoundForGroup(GetComponentInParent<PlayerStats>().idGrupo.Value);
            OnObstaculeCollided?.Invoke(GetComponentInParent<PlayerStats>()? GetComponentInParent<PlayerStats>().idGrupo.Value : 0);
            yield break;
        }
        // Rotacion
        if (direction != Vector3.zero)
        {
            float targetY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetY, 0f);
            float rotElapsed = 0f;
            Quaternion initialRotation = transform.rotation;
            CreateDust();
            while (rotElapsed < moveDuration)
            {
                transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, rotElapsed / moveDuration);
                rotElapsed += Time.deltaTime * rotationSpeed;
                yield return null;
            }
            transform.rotation = targetRotation;
        }
        // Movimiento progresivo
        while (elapsedTime < moveDuration)
        {
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            rb.MovePosition(newPos);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Asegurar posicion final
        rb.MovePosition(targetPos);
        UpdateCurrentGridPos(rb.position);
        IsMoving = false;
        currentMovementCoroutine = null;
    }

    // Metodos llamados por los comandos (solo en servidor o tutorial)
    public void MoveUp()
    {
        if (IsMoving) return;

        if (isTutorialMode)
        {
            TryMoveLocal(Vector3.forward);
        }
        else
        {
            if (!IsServer) return;
            LastMoveBlocked = false;
            targetPosition.Value += Vector3.forward * moveDistance;
        }
    }

    public void MoveDown()
    {
        if (IsMoving) return;

        if (isTutorialMode)
        {
            TryMoveLocal(Vector3.back);
        }
        else
        {
            if (!IsServer) return;
            LastMoveBlocked = false;
            targetPosition.Value += Vector3.back * moveDistance;
        }
    }

    public void MoveLeft()
    {
        if (IsMoving) return;

        if (isTutorialMode)
        {
            TryMoveLocal(Vector3.left);
        }
        else
        {
            if (!IsServer) return;
            LastMoveBlocked = false;
            targetPosition.Value += Vector3.left * moveDistance;
        }
    }

    public void MoveRight()
    {
        if (IsMoving) return;

        if (isTutorialMode)
        {
            TryMoveLocal(Vector3.right);
        }
        else
        {
            if (!IsServer) return;
            LastMoveBlocked = false;
            targetPosition.Value += Vector3.right * moveDistance;
        }
    }

    public void StopMovement()
    {
        if (isTutorialMode)
        {
            TryMoveLocal(Vector3.zero);
            return;
        }
        else
        {
            if (!IsServer) return;
            targetPosition.Value = transform.position;
        }


    }
    private void TryMoveLocal(Vector3 direction)
    {
        if (IsMoving) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 startPos = rb.position;
        Vector3 targetPos = startPos + direction * moveDistance;

        // Resetear flag
        LastMoveBlocked = false;


        // Iniciar movimiento local usando la misma coroutine existente
        if (currentMovementCoroutine == null)
            currentMovementCoroutine = StartCoroutine(MoveToPositionCoroutine(targetPos));
    }

    public override void OnNetworkDespawn()
    {
        targetPosition.OnValueChanged -= OnTargetPositionChanged;
        if (IsServer)
        {
            gameManagerChicken.OnPlayerSpawned -= ActivateIdentificators;
        }
    }
    private void CreateDust()
    {
        dust.Play();
    }
    public bool IsGrounded
    {
        get
        {
            float rayLength = 0.2f;
            Vector3 origin = transform.position + Vector3.up * 0.1f; 
            return Physics.Raycast(origin, Vector3.down, rayLength, LayerMask.GetMask("Default", "Ground"));
        }
    }
    private void UpdateCurrentGridPos(Vector3 worldPos)
    {
        var gen = GridLevelGenerator.Instance;
        if (gen == null) return;

        CurrentGridPos = gen.WorldToGrid(worldPos);
    }
}