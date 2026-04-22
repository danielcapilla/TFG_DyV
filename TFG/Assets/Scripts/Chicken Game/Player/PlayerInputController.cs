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
    public Vector2Int CurrentGridPos { get; set; }
    public bool IsMoving { get;  set; }
    public bool LastMoveBlocked { get;  set; }
    [SerializeField] private LayerMask obstacleMask;
    [Header("Partículas")]
    private ParticleSystem dust;
    [Header("Localizadores")]
    [SerializeField] private Light playerLight;
    [SerializeField] private GameObject halo;
    private GameManagerChicken gameManagerChicken;
    public NetworkVariable<Vector3> targetPosition = new NetworkVariable<Vector3>( Vector3.zero,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Coroutine currentMovementCoroutine;
    private Coroutine speedCoroutine;

    public Action<int> OnObstaculeCollided;

    [Header("Modo tutorial")]
    public bool isTutorialMode = false;

    // Indica si estamos en modo offline (sin red activa)
    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    private void Start()
    {
        dust = GetComponentInChildren<ParticleSystem>();

        // En modo offline OnNetworkSpawn no se llama, inicializamos aqui
        if (IsOffline)
        {
            targetPosition.OnValueChanged += OnTargetPositionChanged;
            gameManagerChicken = FindFirstObjectByType<GameManagerChicken>();
            if (gameManagerChicken != null)
                gameManagerChicken.OnPlayerSpawned += ActivateIdentificators;
            UpdateCurrentGridPos(transform.position);
        }
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
        if (!playerLight.gameObject.activeSelf) return;
        float t = (Mathf.Sin(Time.time * 5f) + 1f) / 2f;
        playerLight.intensity = Mathf.Lerp(0f, 2f, t);
    }

    private void ActivateIdentificators(NetworkObjectReference playerNOR, int idGroup)
    {
        TeamInfo teamInfo = TeamManager.Instance.teams[GetComponentInParent<PlayerStats>().idGrupo.Value];
        ulong[] targetClients = teamInfo.integrantes.ToArray();

        if (IsOffline)
        {
            // En offline activamos directamente sin RPC
            playerLight.gameObject.SetActive(true);
            halo.SetActive(true);
            return;
        }

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
        playerLight.gameObject.SetActive(true);
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
            // En offline no podemos escribir en NetworkVariable, usamos posicion local
            if (!IsOffline)
                targetPosition.Value = transform.position;
            IsMoving = false;
            currentMovementCoroutine = null;
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

    // Metodos llamados por los comandos
    public void MoveUp()
    {
        if (IsMoving) return;
        if (isTutorialMode || IsOffline)
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
        if (isTutorialMode || IsOffline)
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
        if (isTutorialMode || IsOffline)
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
        if (isTutorialMode || IsOffline)
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
        if (isTutorialMode || IsOffline)
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

        LastMoveBlocked = false;

        if (currentMovementCoroutine == null)
            currentMovementCoroutine = StartCoroutine(MoveToPositionCoroutine(targetPos));
    }

    public override void OnNetworkDespawn()
    {
        targetPosition.OnValueChanged -= OnTargetPositionChanged;
        if (IsServer && gameManagerChicken != null)
        {
            gameManagerChicken.OnPlayerSpawned -= ActivateIdentificators;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // Limpieza en modo offline (OnNetworkDespawn no se llama)
        if (IsOffline)
        {
            targetPosition.OnValueChanged -= OnTargetPositionChanged;
            if (gameManagerChicken != null)
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

    public void StartSpeedUp(float multiplier, float duration)
    {
        if (speedCoroutine != null)
            StopCoroutine(speedCoroutine);
        speedCoroutine = StartCoroutine(SpeedUpCoroutine(multiplier, duration));
    }

    private IEnumerator SpeedUpCoroutine(float multiplier, float duration)
    {
        moveDistance = multiplier;
        yield return new WaitForSeconds(duration);
        moveDistance = 1f;
        speedCoroutine = null;
    }
}
