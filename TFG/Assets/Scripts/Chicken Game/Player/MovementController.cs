using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GroupBehaviour group; 

    public void OnLeftButtonClick()
    {
        group.AddCommand(new MoveLeftCommand());
    }

    public void OnRightButtonClick()
    {
        group.AddCommand(new MoveRightCommand());
    }

    public void OnUpButtonClick()
    {
        group.AddCommand(new MoveUpCommand());
    }

    public void OnDownButtonClick()
    {
        group.AddCommand(new MoveDownCommand());
    }

    public void OnEndTurnButtonClick()
    {
        group.ExecuteTurn();
    }
}
