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

    private Graphic graphic;
    private bool isVisible = true;
    private bool hasShaken = false;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
    }

    void Start()
    {
        // Gradiente que respira
        DOTween.To(() => gradient.m_color1,
                   x => { gradient.m_color1 = x; if (graphic != null) graphic.SetAllDirty(); },
                   Color.cyan, 2f)
               .SetLoops(-1, LoopType.Yoyo);

        // Texto "Waiting..."
        waitingText.transform.localScale = Vector3.one;
        waitingText.transform
            .DOScale(1.1f, 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // Animación de entrada
        panelTransform.localScale = Vector3.zero;
        panelTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
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

        panelTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0;

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, 0.3f));
        seq.Join(panelTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        if (shadowBG != null)
            shadowBG.DOColor(new Color(0, 0, 0, 0.5f), 0.3f);
    }

    private void HidePanel()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(0f, 0.25f));
        seq.Join(panelTransform.DOScale(0.9f, 0.25f).SetEase(Ease.InBack));
        seq.OnComplete(() => gameObject.SetActive(false));

        if (shadowBG != null)
            shadowBG.DOColor(new Color(0, 0, 0, 0), 0.25f);
    }

    public void UpdateTimerUI(float currentTime, float totalTime)
    {
        if (timefill == null) return;

        float t = Mathf.Clamp01(currentTime / totalTime);
        Color color = Color.Lerp(Color.red, Color.green, t);
        timefill.color = color;

        // 🌟 Glow dinámico (con pulso suave)
        if (timefillGlow != null)
        {
            // sincroniza el color base del glow con el fill
            Color glowColor = Color.Lerp(color, Color.white, 0.4f);
            timefillGlow.color = glowColor;

            // activa el pulso si no existe ya
            if (!DOTween.IsTweening(timefillGlow))
            {
                timefillGlow
                    .DOFade(0.6f, 1.2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            // opcional: que respire ligeramente en escala también
            if (!DOTween.IsTweening(timefillGlow.transform))
            {
                timefillGlow.transform
                    .DOScale(1.08f, 1.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }


        // 🔔 Pulso del fill cuando queda poco tiempo
        if (currentTime < 3f && !DOTween.IsTweening(timefill.transform))
        {
            timefill.transform
                .DOScale(1.05f, 0.4f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else if (currentTime >= 3f && DOTween.IsTweening(timefill.transform))
        {
            timefill.transform.DOKill();
            timefill.transform.localScale = Vector3.one;
        }

        // 💥 Shake al terminar el tiempo
        if (currentTime <= 0f && !hasShaken)
        {
            hasShaken = true;
            panelTransform.DOShakeAnchorPos(0.6f, 25f, 10, 90f, false, true);
        }
        else if (currentTime > 0f)
        {
            hasShaken = false;
        }
    }
}
