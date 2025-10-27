using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public string localizationKey;

    public bool waitForPlayerAction;
    public TutorialConditionType conditionType = TutorialConditionType.None;

    public MovementPanelAction movementPanelAction = MovementPanelAction.None;

    public ToggleMovementPanelAction toggleMovementPanelAction = ToggleMovementPanelAction.None;

    public float autoAdvanceDelay = 2.5f;

    public bool pauseTimerOnEnter = false;
}

public class TutorialStepsManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Referencias")]
    [SerializeField] private TutorialGroupBehaviour groupBehaviour;
    [SerializeField] private LocalTurnBehaviour turnBehaviour;
    [SerializeField] private ChickenTutorialGameManager gameManager;
    [SerializeField] private UnityEngine.Localization.Components.LocalizeStringEvent tutorialLocalizeStringEvent;
    [SerializeField] private CanvasGroup movementPanel;
    [SerializeField] private Toggle toggle;
    [SerializeField] private PauseChickenTutorial pauseChicken;


    [Header("Pasos del tutorial")]
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

    private int currentStep = 0;

    private Coroutine autoAdvanceCoroutine;
    private MovementPanelBehaviour movementPanelBehaviour;

    public event Action<bool> OnMovementPanelVisibilityChanged;

    void Start()
    {
        toggle.gameObject.GetComponent<CanvasGroup>().alpha = 0;
        movementPanelBehaviour = movementPanel.GetComponent<MovementPanelBehaviour>();
        turnBehaviour.OnPlayerCommandAdded += OnCommandAdded;
        toggle.onValueChanged.AddListener(OnToggleChanged);
        gameManager.OnPlayerReachedGoal += OnReachedGoal;

        ShowStep(0);
    }

    private void OnDestroy()
    {
        turnBehaviour.OnPlayerCommandAdded -= OnCommandAdded;
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

        if (!tutorialPanel.gameObject.activeSelf)
        {
            tutorialPanel.gameObject.SetActive(true);
            tutorialPanel.blocksRaycasts = true;
            tutorialPanel.alpha = 1f;
            tutorialPanel.transform.localScale = Vector3.one;
        }

        tutorialText.DOKill();
        tutorialText
            .DOFade(0, 0.3f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                if (tutorialLocalizeStringEvent != null && !string.IsNullOrEmpty(step.localizationKey))
                {
                    tutorialLocalizeStringEvent.StringReference.TableEntryReference = step.localizationKey;
                    tutorialLocalizeStringEvent.RefreshString();
                }
                else
                {
                    tutorialText.text = step.message;
                }

                tutorialText.DOFade(1, 0.6f).SetEase(Ease.InOutSine);
            });

        tutorialPanel.transform.DOKill();
        tutorialPanel.transform
            .DOScale(1.015f, 2.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        ApplyStepEnterActions(step);

        if (!step.waitForPlayerAction && step.conditionType == TutorialConditionType.None)
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(step.autoAdvanceDelay));
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
                OnMovementPanelVisibilityChanged?.Invoke(false);
            }
            else if (step.movementPanelAction == MovementPanelAction.Show)
            {

                movementPanel.alpha = 1;
                movementPanel.interactable = true;
                movementPanel.blocksRaycasts = true;
                movementPanelBehaviour.isVisible = true;
                OnMovementPanelVisibilityChanged?.Invoke(true);
            }
        }
        if (step.pauseTimerOnEnter)
        {
            turnBehaviour.PauseTurnTimer();
        }
    }

    private void AdvanceStep()
    {
        currentStep++;
        ShowStep(currentStep);
    }


    private void OnCommandAdded()
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
            tutorialPanel.DOKill();

            tutorialPanel.DOFade(0, 0.3f).SetEase(Ease.InOutQuad);
            tutorialPanel.transform.DOScale(0.95f, 0.3f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    tutorialPanel.blocksRaycasts = false;
                    tutorialPanel.gameObject.SetActive(false);
                });
            tutorialPanel.transform.DOKill();
        }

        Debug.Log("Completado");

        if(SceneManager.GetActiveScene().name == "ChickenTutorial 1")
        {
            StartCoroutine(LoadNextSceneAdditiveAndUnloadCurrent("ChickenTutorial 2", "ChickenTutorial 1"));
        }
        else
        {
            pauseChicken.OnQuitButtonClick();
        }
    }
    private IEnumerator LoadNextSceneAdditiveAndUnloadCurrent(string nextSceneName, string prevSceneName)
    {
        // Limpiar el singleton de Grid que si no se lo carga
        //GridLevelGenerator.Instance = null;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;
        
        Scene newScene = SceneManager.GetSceneByName(nextSceneName);
        if (newScene.IsValid())
            SceneManager.SetActiveScene(newScene);
        yield return null;

        yield return SceneManager.UnloadSceneAsync(prevSceneName);
    }
}