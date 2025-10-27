using DG.Tweening;
using System;
using UnityEngine;

public class LocalTurnBehaviour : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private CanvasGroup movementPanel;
    [SerializeField] private CanvasGroup waitingPanel;

    [Header("Referencias")]
    [SerializeField] private TutorialGroupBehaviour groupBehaviour;
    [SerializeField] private TutorialTurnTimer turnTimer;
    [SerializeField] private ChickenTutorialBotsManager botsManager;
    [SerializeField] private TutorialStepsManager stepsManager;

    public bool hasBots = false;

    public event Action OnPlayerCommandAdded;

    void Start()
    {
        turnTimer.OnTimerEnd += HandleTurnTimeout;
        groupBehaviour.OnTurnExecuted += HandleTurnExecuted;
        stepsManager.OnMovementPanelVisibilityChanged += HandleMovementPanelVisibilityChanged;
        
    }
    private void OnDestroy()
    {
        turnTimer.OnTimerEnd -= HandleTurnTimeout;
        groupBehaviour.OnTurnExecuted -= HandleTurnExecuted;
        stepsManager.OnMovementPanelVisibilityChanged -= HandleMovementPanelVisibilityChanged;
    }

    private void HandleTurnTimeout()
    {
        ExecuteTurn(); // se ejecuta el turno al acabarse el tiempo
    }
    private void HandleMovementPanelVisibilityChanged(bool visible)
    {
        if (!visible)
        {
            // Ocultar panel de movimiento
            turnTimer.PauseTimer();
            if (hasBots && botsManager != null)
                botsManager.PauseBots();
            ShowWaiting();
            return;
        }
        if (hasBots && botsManager != null)
        {
            // En Tuto 2 los bots van primero
            ShowWaiting();
            botsManager.RequestBotActions();
            turnTimer.ResetTimer();
        }
        else
        {
            // Tuto 1 solo jugador
            ShowPlayerMoves();
        }
    }

    private void HandleTurnExecuted()
    {

        turnTimer.ResetTimer();
        DOVirtual.DelayedCall(0.1f, ShowMoves);
    }

    public void OnPlayerCommand(CommandType type)
    {
        ShowWaiting();
        ExecuteTurn(); // el jugador actua el ultimo
        OnPlayerCommandAdded?.Invoke();
    }

    private void ExecuteTurn()
    {
        ShowWaiting();
        turnTimer.PauseTimer(); // no contar durante la ejecucion del movimiento
        groupBehaviour.ExecuteTurn(); 
    }

    private void ShowMoves()
    {
        if (hasBots && botsManager != null)
        {
            ShowWaiting();
            botsManager.RequestBotActions(); // cuando acaben, se abrira el panel del player
            return;
        }
        ShowPlayerMoves();
    }

    public void NotifyBotsFinished()
    {
        ShowPlayerMoves();
    }
    public void PauseTurnTimer()
    {
        turnTimer.PauseTimer();
    }

    private void ShowPlayerMoves()
    {
        waitingPanel.DOFade(0, 0.25f);
        waitingPanel.interactable = false;
        waitingPanel.blocksRaycasts = false;

        movementPanel.DOFade(1, 0.3f);
        movementPanel.interactable = true;
        movementPanel.blocksRaycasts = true;
    }

    private void ShowWaiting()
    {
        movementPanel.DOFade(0, 0.25f);
        movementPanel.interactable = false;
        movementPanel.blocksRaycasts = false;

        waitingPanel.DOFade(1, 0.3f);
        waitingPanel.interactable = true;
        waitingPanel.blocksRaycasts = true;
    }
}
