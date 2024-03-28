using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController cController;
    PlayerInput playerInput;
    Vector2 input;
    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Interact"].performed += Interact;
    }

    // Update is called once per frame
    void Update()
    {
        input = playerInput.actions["Movement"].ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        cController.Move(new Vector3(input.x,0,input.y));
    }

    public void Interact(InputAction.CallbackContext context) 
    {
        Debug.Log("Interaccion");
    }
}
