using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTurnTimer : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float timerDuration = 10f;
    [SerializeField] private float delayAfterZero = 1.5f;

    [Header("Referencias")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private MovementPanelBehaviour panelBehaviour;

    private float currentTime;
    private bool isTimerRunning = false;
    private bool isDelayRunning = false;

    public event Action OnTimerEnd;
    public static event Action<float> OnTimerUpdated;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        UpdateTimerDisplay(currentTime);

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            StartCoroutine(DelayAndNextTurn());
        }
    }

    public void ResetTimer()
    {
        currentTime = timerDuration;
        UpdateTimerDisplay(currentTime);
        isTimerRunning = true;
    }

    private IEnumerator DelayAndNextTurn()
    {
        isDelayRunning = true;
        UpdateTimerDisplay(timerDuration);

        yield return new WaitForSeconds(delayAfterZero);

        // Notificamos fin de turno
        OnTimerEnd?.Invoke();

        // Reiniciamos timer para el siguiente turno
        ResetTimer();

        isDelayRunning = false;
    }

    private void UpdateTimerDisplay(float time)
    {
        timerSlider.maxValue = timerDuration;
        timerSlider.value = Mathf.Clamp(time, 0f, timerDuration);

        panelBehaviour?.UpdateTimerUI(time, timerDuration);
        OnTimerUpdated?.Invoke(time / timerDuration);
    }
}
