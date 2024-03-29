using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float force, rotationSpeed;
    Rigidbody rb;
    PlayerInput playerInput;
    Vector2 input;

    Vector3 forward;
    Vector3 right;
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        playerInput.actions["Interact"].performed += Interact;

        cam = Camera.main;

        forward = cam.transform.forward;
        right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        input = playerInput.actions["Movement"].ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        //Vector3 desiredMovement = (forward * input.y + right * input.x);

        Vector3 desiredMovement = new Vector3(input.x, 0f, input.y) * force;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMovement), rotationSpeed * Time.deltaTime);
        //rb.AddForce(new Vector3(input.x, 0f, input.y)*force);
        rb.AddForce(desiredMovement);
    }

    public void Interact(InputAction.CallbackContext context) 
    {
        Debug.Log("Interaccion");
    }
}
