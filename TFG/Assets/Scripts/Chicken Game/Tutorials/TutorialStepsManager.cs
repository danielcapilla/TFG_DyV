using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialConditionType
{
    None,
    CommandAdded,
    TogglePressed,
    ReachedGoal
}

public enum MovementPanelAction
{
    None,
    Show,
    Hide
}
public enum ToggleMovementPanelAction
{
    None,
    Show,
    Hide
}

[Serializable]
public class TutorialStep
{
    [TextArea]
    public string message;

    public bool waitForPlayerAction;
    public TutorialConditionType conditionType = TutorialConditionType.None;

    public MovementPanelAction movementPanelAction = MovementPanelAction.None;

    public ToggleMovementPanelAction toggleMovementPanelAction = ToggleMovementPanelAction.None;

    public float autoAdvanceDelay = 2.5f;
}

public class TutorialStepsManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Referencias")]
    [SerializeField] private TutorialGroupBehaviour groupBehaviour;
    [SerializeField] private ChickenTutorialGameManager gameManager;

    [SerializeField] private CanvasGroup movementPanel;
    [SerializeField] private Toggle toggle;

    [Header("Pasos del tutorial")]
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

    private int currentStep = 0;
    // Para los mensajes con tiempo
    private Coroutine autoAdvanceCoroutine;
    private MovementPanelBehaviour movementPanelBehaviour;

    void Start()
    {
        toggle.gameObject.GetComponent<CanvasGroup>().alpha = 0;
        movementPanelBehaviour = movementPanel.GetComponent<MovementPanelBehaviour>();
        groupBehaviour.OnCommandAdded += OnCommandAdded;
        toggle.onValueChanged.AddListener(OnToggleChanged);
        gameManager.OnPlayerReachedGoal += OnReachedGoal;

        ShowStep(0);
    }

    private void OnDestroy()
    {
        groupBehaviour.OnCommandAdded -= OnCommandAdded;
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
        gameManager.OnPlayerReachedGoal -= OnReachedGoal;

        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
    }

    private void ShowStep(int index)
    {
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        if (index >= steps.Count)
        {
            EndTutorial();
            return;
        }

        var step = steps[index];

        if (tutorialPanel != null)
        {
            tutorialPanel.alpha = 1;
            tutorialPanel.blocksRaycasts = true;
        }

        if (tutorialText != null)
            tutorialText.text = step.message;

        ApplyStepEnterActions(step);
        if (!step.waitForPlayerAction && step.conditionType == TutorialConditionType.None)
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(step.autoAdvanceDelay));;
    }
    private void AdvanceStepWithDelay(float delay)
    {
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
        autoAdvanceCoroutine = StartCoroutine(AdvanceStepAfterDelay(delay));
    }

    private IEnumerator AdvanceStepAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
        AdvanceStep();
    }
    private IEnumerator AutoAdvanceAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        AdvanceStep();
    }

    private void ApplyStepEnterActions(TutorialStep step)
    {
        if (toggle != null)
        {
            if (step.toggleMovementPanelAction == ToggleMovementPanelAction.Hide)
                toggle.gameObject.GetComponent<CanvasGroup>().alpha = 0;
            else if (step.toggleMovementPanelAction == ToggleMovementPanelAction.Show)
            {
                toggle.gameObject.GetComponent<CanvasGroup>().alpha = 1;
            }
        }
        if (movementPanel != null)
        {
            bool panelCurrentlyActive = movementPanelBehaviour.isVisible;

            if (step.movementPanelAction == MovementPanelAction.Hide)
            {
                    movementPanel.alpha = 0;
                    movementPanel.interactable = false;
                    movementPanel.blocksRaycasts = false;
                    movementPanelBehaviour.isVisible = false;

            }
            else if (step.movementPanelAction == MovementPanelAction.Show)
            {

                    movementPanel.alpha = 1;
                    movementPanel.interactable = true;
                    movementPanel.blocksRaycasts = true;
                    movementPanelBehaviour.isVisible = true;

            }
        }
    }

    private void AdvanceStep()
    {
        currentStep++;
        ShowStep(currentStep);
    }

    // === EVENTOS ===

    private void OnCommandAdded(CommandType type)
    {
        if (currentStep >= steps.Count) return;

        if (steps[currentStep].conditionType == TutorialConditionType.CommandAdded)
        {
            Debug.Log("[Tutorial] Paso completado: Comando añadido");
            AdvanceStepWithDelay(steps[currentStep].autoAdvanceDelay);
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        if (currentStep >= steps.Count) return;

        if (steps[currentStep].conditionType == TutorialConditionType.TogglePressed)
        {
            Debug.Log("[Tutorial] Paso completado: Toggle activado");
            AdvanceStepWithDelay(steps[currentStep].autoAdvanceDelay);
        }
    }

    private void OnReachedGoal()
    {
        if (currentStep >= steps.Count) return;

        if (steps[currentStep].conditionType == TutorialConditionType.ReachedGoal)
        {
            Debug.Log("[Tutorial] Paso completado: Meta alcanzada");
            AdvanceStepWithDelay(steps[currentStep].autoAdvanceDelay);
        }
    }

    private void EndTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.alpha = 0;
            tutorialPanel.blocksRaycasts = false;
        }

        // Reactivar el Toggle al finalizar el tutorial
        if (toggle != null)
            toggle.interactable = true;

        Debug.Log("Completado");
    }
}