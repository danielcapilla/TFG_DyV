using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovementPanelBehaviour : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private UIGradient gradient;
    [SerializeField] private TextMeshProUGUI waitingText;
    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image shadowBG;
    [SerializeField] private Image timefill;
    [SerializeField] private Image timefillGlow;
    [SerializeField] private GameObject movementButton;

    private Graphic graphic;
    public bool isVisible = true;
    private bool hasShaken = false;

    // Tweens registrados (para la limpieza)
    private Tweener gradientTween;
    private Tweener waitingScaleTween;
    private Tweener introScaleTween;
    private Tweener introFadeTween;
    private Tweener shadowFadeTween;
    private Tweener glowFadeTween;
    private Tweener glowScaleTween;
    private Tweener lowTimePulseTween;
    private Tweener shakeTween;
    private Tweener movementButtonTween;

    void Awake()
    {
        graphic = GetComponent<Graphic>();
    }

    void Start()
    {
        // Gradiente que respira
        if (gradient != null)
        {
            gradientTween = DOTween.To(
                () => gradient.m_color1,
                x => { gradient.m_color1 = x; if (graphic != null) graphic.SetAllDirty(); },
                Color.cyan,
                2f
            ).SetLoops(-1, LoopType.Yoyo);
        }

        // Texto "Waiting..."
        if (waitingText != null)
        {
            waitingText.transform.localScale = Vector3.one;
            waitingScaleTween = waitingText.transform
                .DOScale(1.1f, 0.8f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        // Animacion de entrada
        if (panelTransform != null)
        {
            panelTransform.localScale = Vector3.zero;
            introScaleTween = panelTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack)
                .OnComplete(AnimateMovementButtonPop); ;
        }
    }

    public void TogglePanel()
    {
        if (isVisible)
            HidePanel();
        else
            ShowPanel();
        isVisible = !isVisible;
    }

    private void ShowPanel()
    {
        gameObject.SetActive(true);

        if (panelTransform != null)
        {
            panelTransform.localScale = Vector3.zero;
            introScaleTween?.Kill();
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            introFadeTween?.Kill();
        }

        if (canvasGroup != null)
            introFadeTween = canvasGroup.DOFade(1f, 0.3f);
        if (panelTransform != null)
            introScaleTween = panelTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

        if (shadowBG != null)
        {
            shadowFadeTween?.Kill();
            shadowFadeTween = shadowBG.DOColor(new Color(0, 0, 0, 0.5f), 0.3f);
        }
    }

    private void HidePanel()
    {
        Sequence seq = DOTween.Sequence();

        if (canvasGroup != null)
            seq.Append(canvasGroup.DOFade(0f, 0.25f));
        if (panelTransform != null)
            seq.Join(panelTransform.DOScale(0.9f, 0.25f).SetEase(Ease.InBack));

        seq.OnComplete(() => gameObject.SetActive(false));

        if (shadowBG != null)
        {
            shadowFadeTween?.Kill();
            shadowFadeTween = shadowBG.DOColor(new Color(0, 0, 0, 0), 0.25f);
        }
    }

    public void UpdateTimerUI(float currentTime, float totalTime)
    {
        if (timefill == null) return;

        float t = Mathf.Clamp01(currentTime / totalTime);
        Color color = Color.Lerp(Color.red, Color.green, t);
        timefill.color = color;

        if (timefillGlow != null)
        {
            Color glowColor = Color.Lerp(color, Color.white, 0.4f);
            timefillGlow.color = glowColor;

            if (glowFadeTween == null || !glowFadeTween.IsActive())
            {
                glowFadeTween = timefillGlow
                    .DOFade(0.6f, 1.2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
            if (glowScaleTween == null || !glowScaleTween.IsActive())
            {
                glowScaleTween = timefillGlow.transform
                    .DOScale(1.08f, 1.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        if (currentTime < 3f)
        {
            if (lowTimePulseTween == null || !lowTimePulseTween.IsActive())
            {
                lowTimePulseTween = timefill.transform
                    .DOScale(1.05f, 0.4f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        else
        {
            if (lowTimePulseTween != null && lowTimePulseTween.IsActive())
            {
                lowTimePulseTween.Kill();
                timefill.transform.localScale = Vector3.one;
            }
        }

        if (currentTime <= 0f && !hasShaken)
        {
            hasShaken = true;
            if (panelTransform != null)
            {
                shakeTween?.Kill();
                shakeTween = panelTransform.DOShakeAnchorPos(0.6f, 25f, 10, 90f, false, true);
            }
        }
        else if (currentTime > 0f)
        {
            hasShaken = false;
        }
    }
    private void AnimateMovementButtonPop()
    {
        if (movementButton == null) return;

        RectTransform rt = movementButton.GetComponent<RectTransform>();
        if (rt == null) return;

        movementButtonTween?.Kill();

        rt.gameObject.SetActive(true);
        rt.localScale = Vector3.zero;

        movementButtonTween = rt
            .DOScale(1f, 0.35f)
            .SetEase(Ease.OutBack); // Pop sencillo
    }

    void OnDisable()
    {
        KillAllTweens();
    }

    void OnDestroy()
    {
        KillAllTweens();
    }

    private void KillAllTweens()
    {
        gradientTween?.Kill();
        waitingScaleTween?.Kill();
        introScaleTween?.Kill();
        introFadeTween?.Kill();
        shadowFadeTween?.Kill();
        glowFadeTween?.Kill();
        glowScaleTween?.Kill();
        lowTimePulseTween?.Kill();
        shakeTween?.Kill();

        if (panelTransform) DOTween.Kill(panelTransform);
        if (canvasGroup) DOTween.Kill(canvasGroup);
        if (waitingText) DOTween.Kill(waitingText.transform);
        if (timefill) DOTween.Kill(timefill.transform);
        if (timefillGlow)
        {
            DOTween.Kill(timefillGlow);
            DOTween.Kill(timefillGlow.transform);
        }
        if (gradient) DOTween.Kill(gradient);
        if (movementButton)
        {
            DOTween.Kill(movementButton);
            DOTween.Kill(movementButton.transform);
        }
    }
}