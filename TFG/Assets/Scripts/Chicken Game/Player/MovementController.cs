using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GroupBehaviour group;

    public void OnLeftButtonClick()
    {
        group.AddCommand(CommandType.MoveLeft);
    }

    public void OnRightButtonClick()
    {
        group.AddCommand(CommandType.MoveRight);
    }

    public void OnUpButtonClick()
    {
        group.AddCommand(CommandType.MoveUp);
    }

    public void OnDownButtonClick()
    {
        group.AddCommand(CommandType.MoveDown);
    }

    public void OnWaitButtonClick()
    {
        group.AddCommand(CommandType.Wait);
    }

}
