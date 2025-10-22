using DG.Tweening;
using UnityEngine;

public class LocalTurnBehaviour : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private CanvasGroup movementPanel;
    [SerializeField] private CanvasGroup waitingPanel;

    [Header("Referencias")]
    [SerializeField] private TutorialGroupBehaviour groupBehaviour;
    [SerializeField] private TutorialTurnTimer turnTimer;

    private bool playerHasActed = false;
    private bool botHasActed = false;
    public bool hasBots = false;

    void Start()
    {
        turnTimer.OnTimerEnd += HandleTurnTimeout;
        groupBehaviour.OnCommandAdded += OnPlayerCommand;
        ShowMoves(); 
    }
    private void OnDestroy()
    {
        turnTimer.OnTimerEnd -= HandleTurnTimeout;
        groupBehaviour.OnCommandAdded -= OnPlayerCommand;
    }
    private void HandleTurnTimeout()
    {
        ExecuteTurn();
    }

    public void OnPlayerCommand(CommandType type)
    {
        playerHasActed = true;
        ShowWaiting();

        if (!hasBots)
            ExecuteTurn(); // en el tutorial 1
        else
            CheckIfTurnFinished(); // en el 2
    }

    private void CheckIfTurnFinished()
    {
        bool allDone = playerHasActed && botHasActed;
        if (allDone)
            ExecuteTurn();
    }

    private void ExecuteTurn()
    {
        ShowWaiting();
        groupBehaviour.ExecuteTurn(); 
        turnTimer.ResetTimer();       
        playerHasActed = false;
        botHasActed = false;

        DOVirtual.DelayedCall(1.0f, ShowMoves);
    }

    private void ShowMoves()
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
