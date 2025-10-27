using UnityEngine;

public class TutorialCommandController : MonoBehaviour
{
    [SerializeField] private TutorialGroupBehaviour group;
    [SerializeField] private LocalTurnBehaviour localTurnBehaviour; 

    public void OnLeftButtonClick()
    {
        group.AddCommand(CommandType.MoveLeft);
        localTurnBehaviour?.OnPlayerCommand(CommandType.MoveLeft);
    }

    public void OnRightButtonClick()
    {
        group.AddCommand(CommandType.MoveRight);
        localTurnBehaviour?.OnPlayerCommand(CommandType.MoveRight);
    }

    public void OnUpButtonClick()
    {
        group.AddCommand(CommandType.MoveUp);
        localTurnBehaviour?.OnPlayerCommand(CommandType.MoveUp);
    }

    public void OnDownButtonClick()
    {
        group.AddCommand(CommandType.MoveDown);
        localTurnBehaviour?.OnPlayerCommand(CommandType.MoveDown);
    }

    public void OnWaitButtonClick()
    {
        group.AddCommand(CommandType.Wait);
        localTurnBehaviour?.OnPlayerCommand(CommandType.Wait);
    }
}
