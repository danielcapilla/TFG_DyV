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
    public event Action OnTurnExecuted;

    private bool isExecuting = false;

    private void Awake()
    {
        gameManager.OnPlayerSpawned += OnPlayerSpawned;
    }
    private void OnDestroy()
    {
        if (gameManager != null)
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
        if (!isExecuting)
            StartCoroutine(ExecuteTurnCoroutine());
    }

    private IEnumerator ExecuteTurnCoroutine()
    {
        isExecuting = true;

        // Snapshot de comandos y limpieza, igual que en online
        var turnCommands = new List<ICommand>(commandQueue);
        commandQueue.Clear();

        foreach (var command in turnCommands)
        {
            command.Execute(playerController);

            // Esperar inicio y fin de movimiento si aplica
            yield return new WaitForSeconds(0.05f);
            if (playerController.IsMoving)
                yield return new WaitUntil(() => !playerController.IsMoving);

            if (playerController.LastMoveBlocked)
            {
                // Igual que online: romper el turno, pero SIEMPRE notificar OnTurnExecuted después
                break;
            }

            // Margen suave entre comandos, como en online
            yield return new WaitForSeconds(0.05f);
        }

        OnTurnExecuted?.Invoke();
        isExecuting = false;
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
