using UnityEngine;


public class MoveLeftCommand : ICommand
{
    public void Execute(PlayerInputController player)
    {
        player.MoveLeft();
    }
}

public class MoveRightCommand : ICommand
{
    public void Execute(PlayerInputController player)
    {
        player.MoveRight();
    }
}

public class MoveUpCommand : ICommand
{
    public void Execute(PlayerInputController player)
    {
        player.MoveUp();
    }
}

public class MoveDownCommand : ICommand
{
    public void Execute(PlayerInputController player)
    {
        player.MoveDown();
    }
}

public class WaitCommand : ICommand
{
    public void Execute(PlayerInputController player)
    {
        player.StopMovement();
    }
}