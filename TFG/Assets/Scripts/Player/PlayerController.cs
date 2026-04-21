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

    PlayerCarry carryScript;
    LayerMask layer;

    public InteractableObject interactableInRange;

    [SerializeField] GameObject FeetLocalizer;

    // Sin red activa, actuamos como si fueramos el owner
    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;
    private bool IsLocallyControlled => IsOffline || IsOwner;

    private void Start()
    {
        // En offline OnNetworkSpawn no se llama, inicializamos todo aqui
        if (IsOffline)
        {
            InitializePlayer();
        }
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
        if (FeetLocalizer != null)
            FeetLocalizer.SetActive(true);

        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
        rb = GetComponent<Rigidbody>();
        playerInput.actions["Interact"].performed += Interact;

        cam = Camera.main;

        forward = cam.transform.forward;
        right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        carryScript = GetComponent<PlayerCarry>();

        layer = gameObject.layer;
        layer = 1 << layer;
        layer = ~layer;
    }

    void Update()
    {
        if (!IsLocallyControlled) return;
        if (playerInput == null) return;

        input = playerInput.actions["Movement"].ReadValue<Vector2>();

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, InteractionRange, layer))
        {
            if (hit.transform.gameObject.TryGetComponent<InteractableObject>(out InteractableObject interactable))
            {
                if (interactable != interactableInRange && interactableInRange != null)
                {
                    interactableInRange.toggleHighlight(false);
                }
                interactable.toggleHighlight(true);
                interactableInRange = interactable;
            }
        }
        else
        {
            if (interactableInRange != null)
            {
                interactableInRange.toggleHighlight(false);
                interactableInRange = null;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsLocallyControlled) return;
        if (rb == null) return;

        Vector3 desiredMovement = new Vector3(input.x, 0f, input.y);
        if (desiredMovement.magnitude > 0.1f)
        {
            rb.AddForce(desiredMovement * force);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMovement, Vector3.up), rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.forward, Vector3.up), rotationSpeed * Time.deltaTime);
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("Interaccion");
        if (interactableInRange != null)
        {
            interactableInRange.Interact(carryScript);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.TransformDirection(Vector3.forward) * InteractionRange + transform.position);
    }
}
