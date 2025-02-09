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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        DontDestroyOnLoad(gameObject);
        if (!IsOwner) return;
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
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

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
        if (!IsOwner) return;
        //Vector3 desiredMovement = (forward * input.y + right * input.x);

        Vector3 desiredMovement = new Vector3(input.x, 0f, input.y);
        if (desiredMovement.magnitude > 0.1f)
        {
            rb.AddForce(desiredMovement * force);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMovement, Vector3.up), rotationSpeed * Time.deltaTime);
            //rb.AddForce(new Vector3(input.x, 0f, input.y)*force);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.forward, Vector3.up), rotationSpeed * Time.deltaTime);
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("Interaccion");

        //Check for interactable object in front and call its interact method
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
