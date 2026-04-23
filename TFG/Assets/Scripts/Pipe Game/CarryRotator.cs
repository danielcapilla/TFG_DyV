using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rota el objeto que lleva el jugador si implementa IRotatableObject.
/// Requiere PlayerCarry y PlayerInput en el mismo GameObject.
/// La accion de input se llama 'Rotate'.
/// </summary>
[RequireComponent(typeof(PlayerCarry))]
public class CarryRotator : MonoBehaviour
{
    private PlayerCarry carry;
    private PlayerInput playerInput;

    private void Awake()
    {
        carry = GetComponent<PlayerCarry>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput != null)
            playerInput.actions["Rotate"].performed += Rotate;
    }

    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.actions["Rotate"].performed -= Rotate;
    }

    public void Rotate(InputAction.CallbackContext context)
    {
        if (!carry.isCarrying) return;

        carry.carryingObject
            ?.GetGameObject()
            ?.GetComponent<IRotatableObject>()
            ?.Rotate90();
    }
}
