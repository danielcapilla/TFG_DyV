using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTurnTimer : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float timerDuration = 10f;

    [Header("Referencias")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private MovementPanelBehaviour panelBehaviour;

    private float currentTime;
    private bool isTimerRunning = false;

    public event Action OnTimerEnd;
    public static event Action<float> OnTimerUpdated;

    void Start()
    {
        UpdateTimerDisplay(timerDuration);
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

            OnTimerEnd?.Invoke();
        }
    }

    public void ResetTimer()
    {
        currentTime = timerDuration;
        UpdateTimerDisplay(currentTime);
        isTimerRunning = true;
    }

    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    private void UpdateTimerDisplay(float time)
    {
        timerSlider.maxValue = timerDuration;
        timerSlider.value = Mathf.Clamp(time, 0f, timerDuration);

        panelBehaviour?.UpdateTimerUI(time, timerDuration);
        OnTimerUpdated?.Invoke(timerDuration > 0f ? Mathf.Clamp01(time / timerDuration) : 0f);
    }
}
