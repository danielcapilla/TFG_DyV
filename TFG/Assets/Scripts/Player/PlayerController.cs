using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Experimental.GraphView;
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

    // Database
    Timer interactionTimer;

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
        interactionTimer = new Timer();
        interactionTimer.StartTimer();
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

            // Interaction collected for database
            // Time class in this script
            int time = (int)interactionTimer.GetElapsedTime();

            // Interaction type can be collected from "carryScript"
            int interactionType = carryScript.isCarrying ? 1 : 0;
            // Necessary interaction can be collected from "interactableInRange"

            NecessaryInteractionType();
            // Object interacted with can be collected from "interactableInRange"
            // Deliver can be collected from "carryScript"
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.TransformDirection(Vector3.forward) * InteractionRange + transform.position);
    }

    private int NecessaryInteractionType()
    {
        int necessaryInteraction = 0;

        // Check if carrying something
        if (carryScript.isCarrying)
        {
            GameObject carriedGO = carryScript.carryingObject.GetGameObject();
            Debug.Log("CARRIED GO: " + carriedGO.name);
            if (interactableInRange.gameObject.TryGetComponent<DeliveryStation>(out DeliveryStation deliveryStation)) return 2;
            if (carriedGO.TryGetComponent<PlateBehaviour>(out PlateBehaviour plate))
            {
                return 1; // Deliver plate
            }
            else // If not a plate, must be an ingredient
            {
                IngredientBehaviour ingredient = carriedGO.GetComponent<IngredientBehaviour>();
                if (ingredient.ingredient.ID == 0)
                {
                    return 1; // Deliver bread
                }
                else
                {
                    if (!CheckIfHamburguerOnTable())
                    {
                        return 0; // If there is NOT a plate on the table return 0
                    }
                    PlateBehaviour hamburguer = interactableInRange.gameObject.GetComponent<Table>().holdingObject.GetGameObject().GetComponent<PlateBehaviour>();
                    if (CheckIfCorrectIngredient(ingredient, hamburguer)) return 1; // Correct ingredient
                    return 0; // Incorrect ingredient

                }
            }
        }
        else // Carrying nothing
        {
            Debug.Log("NOT CARRYING ANYTHING");
            return 3;
        }

    }
    
    private bool CheckIfHamburguerOnTable()
    {
        InteractableObject interactableObject = interactableInRange;
        if (interactableObject.gameObject.TryGetComponent<Table>(out Table table))
        {
            if (table.isOccupied)
            {
                if(table.holdingObject.GetGameObject().TryGetComponent<PlateBehaviour>(out PlateBehaviour plate))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if an ingredient is part of the ordered hamburguer
    /// </summary>
    /// <param name="ingredient"></param>
    /// <param name="hamburguer"></param>
    /// <returns></returns>
    private bool CheckIfCorrectIngredient(IngredientBehaviour newIngredient, PlateBehaviour hamburguer)
    {
        // Get ingredients on the plate
        List<IngredientsScriptableObject> hamburguerIngredients = new List<IngredientsScriptableObject>();
        List<IngredientsScriptableObject> requiredIngredients = FindFirstObjectByType<RecipeRandomizer>().GetCurrentOrder();

        List<IngredientBehaviour> ingredientCurrentHamburguer = hamburguer.GetComponentsInChildren<IngredientBehaviour>().ToList();
        foreach (IngredientBehaviour ingredient in ingredientCurrentHamburguer)
        {
            hamburguerIngredients.Add(ingredient.ingredient);
        }

        // Check that every ingredient on current hamburguer is part of the required ingredients
        int i = 0;
        if(hamburguerIngredients.Count > requiredIngredients.Count)
        {
            Debug.Log("Hamburguer has more ingredients than required");
            return false;
        }
        for (i = 0; i < hamburguerIngredients.Count; i++)
        {
            Debug.Log("Comparing ingredient ID " + hamburguerIngredients[i].ID + " with required ingredient ID " + requiredIngredients[i].ID);
            if (hamburguerIngredients[i].ID != requiredIngredients[i].ID) return false;
        }

        return true;
    }
}
