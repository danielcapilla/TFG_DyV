using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : NetworkBehaviour
{
    [Header("Variables de movimiento")]
    public float force = 10f;
    public float rotationSpeed = 10f;
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 movement;
    Rigidbody rb;
    Vector2 input;
    PlayerInput playerInput;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    public void MoveLeft()
    {
        movement = Vector2.left;
    }

    public void MoveRight()
    {
        movement = Vector2.right;
    }

    public void MoveUp()
    {
        movement = Vector2.up;
    }

    public void MoveDown()
    {
        movement = Vector2.down;
    }

    public void StopMovement()
    {
        movement = Vector2.zero;
        rb.linearVelocity = Vector3.zero;
    }

    void Update()
    {
        if (!IsOwner) return;
        //input = playerInput.actions["Movement"].ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (movement != Vector2.zero)
        {
            Vector3 desiredMovement = new Vector3(movement.x, 0f, movement.y).normalized;
            rb.AddForce(desiredMovement * force, ForceMode.Force);

            // Rotar hacia la direccion de movimiento
            Quaternion targetRotation = Quaternion.LookRotation(desiredMovement, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}