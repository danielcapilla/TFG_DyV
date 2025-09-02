using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerInputController : NetworkBehaviour
{
    [Header("Variables de movimiento")]
    public float force = 10f;
    public float rotationSpeed = 10f;

    Rigidbody rb;
    Vector2 input;
    PlayerInput playerInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }
    override public void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) return;    
        input = playerInput.actions["Movement"].ReadValue<Vector2>();
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
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
}
