using System.Collections;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GroupBehaviour group;

    [Header("Power-Ups")]
    private bool controlsInverted = false;
    private Coroutine invertControlsCoroutine;

    private void OnEnable()
    {
        PowerUpEvents.OnInvertControls += InvertControls;
    }
    private void OnDisable()
    {
        PowerUpEvents.OnInvertControls -= InvertControls;
    }
    public void OnLeftButtonClick()
    {
        CommandType command = controlsInverted ? CommandType.MoveRight : CommandType.MoveLeft;
        group.AddCommand(command);
    }

    public void OnRightButtonClick()
    {
        CommandType command = controlsInverted ? CommandType.MoveLeft : CommandType.MoveRight;
        group.AddCommand(command);
    }

    public void OnUpButtonClick()
    {
        CommandType command = controlsInverted ? CommandType.MoveDown : CommandType.MoveUp;
        group.AddCommand(command);
    }

    public void OnDownButtonClick()
    {
        CommandType command = controlsInverted ? CommandType.MoveUp : CommandType.MoveDown;
        group.AddCommand(command);
    }

    public void OnWaitButtonClick()
    {
        group.AddCommand(CommandType.Wait); 
    }
    public void InvertControls(float duration)
    {
        // Si ya hay una inversion activa, cancelarla y empezar una nueva
        if (invertControlsCoroutine != null)
        {
            StopCoroutine(invertControlsCoroutine);
        }

        invertControlsCoroutine = StartCoroutine(InvertControlsCoroutine(duration));
    }

    private IEnumerator InvertControlsCoroutine(float duration)
    {
        controlsInverted = true;
        Debug.Log($"[MovementController] Controls inverted for {duration} seconds");

        yield return new WaitForSeconds(duration);
        // Parar la inversion
        controlsInverted = false;
        Debug.Log($"[MovementController] Controls restored to normal");

    }
}
