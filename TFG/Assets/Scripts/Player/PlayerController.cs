using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] float force, rotationSpeed, InteractionRange;
    Rigidbody rb;
    PlayerInput playerInput;
    Vector2 input;

    Vector3 forward;
    Vector3 right;
    Camera cam;

    LayerMask layer;

    public InteractableObject interactableInRange;

    [SerializeField] GameObject FeetLocalizer;

    [Header("Deteccion de interactuables")]
    [SerializeField][Range(10f, 180f)] private float interactionAngle = 90f;

    private Vector3 lastMoveDirection = Vector3.forward;

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;
    private bool IsLocallyControlled => IsOffline || IsOwner;

    private void Start()
    {
        if (IsOffline) InitializePlayer();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        DontDestroyOnLoad(gameObject);
        if (!IsOwner) return;
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        if (FeetLocalizer != null) FeetLocalizer.SetActive(true);

        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
        rb = GetComponent<Rigidbody>();
        playerInput.actions["Interact"].performed += Interact;

        cam = Camera.main;
        forward = cam.transform.forward;
        right = cam.transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        layer = gameObject.layer;
        layer = 1 << layer;
        layer = ~layer;
    }

    void Update()
    {
        if (!IsLocallyControlled) return;
        if (playerInput == null) return;

        input = playerInput.actions["Movement"].ReadValue<Vector2>();

        // Actualizar ultima direccion de movimiento
        Vector3 moveDir = new Vector3(input.x, 0f, input.y);
        if (moveDir.magnitude > 0.1f)
            lastMoveDirection = moveDir.normalized;

        InteractableObject detected = DetectInteractable();

        if (detected != interactableInRange)
        {
            if (interactableInRange != null) interactableInRange.toggleHighlight(false);
            interactableInRange = detected;
            if (interactableInRange != null) interactableInRange.toggleHighlight(true);
        }
    }

    private InteractableObject DetectInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, InteractionRange, layer);

        InteractableObject best = null;
        float bestScore = -1f;

        foreach (Collider col in hits)
        {
            if (!col.TryGetComponent<InteractableObject>(out InteractableObject interactable)) continue;

            Vector3 toObject = col.transform.position - transform.position;
            toObject.y = 0f;
            if (toObject.sqrMagnitude < 0.001f) continue;

            float angle = Vector3.Angle(lastMoveDirection, toObject.normalized);
            if (angle > interactionAngle * 0.5f) continue;

            float score = (1f - angle / (interactionAngle * 0.5f)) / toObject.magnitude;
            if (score > bestScore)
            {
                bestScore = score;
                best = interactable;
            }
        }

        return best;
    }

    private void FixedUpdate()
    {
        if (!IsLocallyControlled) return;
        if (rb == null) return;

        Vector3 desiredMovement = new Vector3(input.x, 0f, input.y);
        if (desiredMovement.magnitude > 0.1f)
        {
            rb.AddForce(desiredMovement * force);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(desiredMovement, Vector3.up), rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(transform.forward, Vector3.up), rotationSpeed * Time.deltaTime);
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (interactableInRange != null)
            interactableInRange.Interact(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, InteractionRange);

        Gizmos.color = Color.cyan;
        Vector3 dir = Application.isPlaying ? lastMoveDirection : transform.forward;
        Vector3 left  = Quaternion.Euler(0,  interactionAngle * 0.5f, 0) * dir * InteractionRange;
        Vector3 right2 = Quaternion.Euler(0, -interactionAngle * 0.5f, 0) * dir * InteractionRange;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right2);
        Gizmos.DrawLine(transform.position, transform.position + dir * InteractionRange);
    }
}
