using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGroupBehaviour : MonoBehaviour
{
    private PlayerInputController playerController;
    private readonly Queue<ICommand> commandQueue = new Queue<ICommand>();
    [SerializeField] private ChickenTutorialGameManager gameManager;
    public event Action<CommandType> OnCommandAdded;

    private void Awake()
    {
        gameManager.OnPlayerSpawned += OnPlayerSpawned;
    }
    private void OnDestroy()
    {
        gameManager.OnPlayerSpawned -= OnPlayerSpawned;
    }
    private void OnPlayerSpawned(PlayerInputController controller)
    {
        playerController = controller;
    }

    public void AddCommand(CommandType type)
    {
        ICommand command = CreateCommandFromType(type);
        commandQueue.Enqueue(command);
        OnCommandAdded?.Invoke(type);

    }

    public void ExecuteTurn()
    {
        StartCoroutine(ExecuteTurnCoroutine());
    }

    private IEnumerator ExecuteTurnCoroutine()
    {
        while (commandQueue.Count > 0)
        {
            var command = commandQueue.Dequeue();
            command.Execute(playerController);

            // Esperar a que termine el movimiento si aplica
            yield return new WaitForSeconds(0.05f);
            if (playerController.IsMoving)
                yield return new WaitUntil(() => !playerController.IsMoving);

            if (playerController.LastMoveBlocked)
            {
                yield break;
            }
        }
    }

    private ICommand CreateCommandFromType(CommandType type)
    {
        switch (type)
        {
            case CommandType.MoveLeft: return new MoveLeftCommand();
            case CommandType.MoveRight: return new MoveRightCommand();
            case CommandType.MoveUp: return new MoveUpCommand();
            case CommandType.MoveDown: return new MoveDownCommand();
            case CommandType.Wait: return new WaitCommand();
            default: return new WaitCommand();
        }
    }
}
