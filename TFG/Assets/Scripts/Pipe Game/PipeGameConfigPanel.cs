using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel de configuracion del Pipe Game.
/// Expone al usuario solo el tamaño del grid, la dificultad y el tiempo.
/// Los parametros internos (gapRatio, lockRatio, reserveSlots, extraTiles)
/// se calculan automaticamente segun el nivel de dificultad y el tamaño del grid.
/// </summary>
public class PipeGameConfigPanel : MonoBehaviour, IConfigPanel
{
    [Header("Tamaño del grid")]
    [SerializeField] private Slider gridSizeSlider;
    [SerializeField] private TextMeshProUGUI gridSizeLabel;

    [Header("Dificultad")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button expertButton;

    [Header("Tiempo")]
    [SerializeField] private Slider durationSlider;
    [SerializeField] private TextMeshProUGUI durationLabel;

    [Header("Cerrar")]
    [SerializeField] private Button closeButton;

    // Definicion de niveles de dificultad
    private struct DifficultyPreset
    {
        public float gapRatio;
        public float lockRatio;
        public int   extraTiles;

        public DifficultyPreset(float gap, float lockR, int extra)
        {
            gapRatio  = gap;
            lockRatio = lockR;
            extraTiles = extra;
        }
    }

    private static readonly string[] DifficultyNames = new[] { "Facil", "Normal", "Dificil", "Experto" };

    private static readonly DifficultyPreset[] Presets = new[]
    {
        new DifficultyPreset(0.50f, 0.10f, 5), // Facil:   muchos huecos, casi sin bloqueadas
        new DifficultyPreset(0.35f, 0.35f, 3), // Normal
        new DifficultyPreset(0.25f, 0.60f, 1), // Dificil
        new DifficultyPreset(0.40f, 0.80f, 0), // Experto: muchos huecos Y muchas bloqueadas
    };

    private PipeGameConfigSO config;
    private int currentDifficulty = 1; // Normal por defecto
    private Button[] difficultyButtons;

    public void Setup(GameConfigBaseSO baseConfig)
    {
        config = baseConfig as PipeGameConfigSO;
        if (config == null) return;

        // ── Slider tamaño del grid (4-8, mismo valor para cols y rows) ────────
        if (gridSizeSlider != null)
        {
            gridSizeSlider.minValue     = 4;
            gridSizeSlider.maxValue     = 8;
            gridSizeSlider.wholeNumbers = true;
            gridSizeSlider.value        = Mathf.Max(config.columns, config.rows);
            gridSizeSlider.onValueChanged.AddListener(OnGridSizeChanged);
        }

        // ── Slider tiempo (60-300, intervalos de 30s) ─────────────────────────
        if (durationSlider != null)
        {
            durationSlider.minValue     = 60f;
            durationSlider.maxValue     = 300f;
            durationSlider.wholeNumbers = false;
            durationSlider.value        = config.matchDuration;
            durationSlider.onValueChanged.AddListener(OnDurationChanged);
        }

        // ── Botones de dificultad ─────────────────────────────────────────────
        difficultyButtons = new[] { easyButton, normalButton, hardButton, expertButton };
        if (easyButton   != null) easyButton  .onClick.AddListener(() => SetDifficulty(0));
        if (normalButton != null) normalButton.onClick.AddListener(() => SetDifficulty(1));
        if (hardButton   != null) hardButton  .onClick.AddListener(() => SetDifficulty(2));
        if (expertButton != null) expertButton.onClick.AddListener(() => SetDifficulty(3));

        // ── Boton cerrar ──────────────────────────────────────────────────────
        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
                MinigameSelectorBehaviour.Instance?.CloseConfigPanel());

        // Aplicar valores iniciales
        SetDifficulty(currentDifficulty);
        UpdateLabels();
    }

    private void OnGridSizeChanged(float value)
    {
        int size = Mathf.RoundToInt(value);
        config.columns = size;
        config.rows    = size;
        // Recalcular reserveSlots con la dificultad actual
        ApplyPreset(currentDifficulty);
        UpdateLabels();
    }

    private void OnDurationChanged(float value)
    {
        // Redondear al multiplo de 30 mas cercano
        float snapped = Mathf.Round(value / 30f) * 30f;
        config.matchDuration = snapped;
        if (durationSlider != null) durationSlider.value = snapped;
        if (durationLabel  != null) durationLabel.text   = FormatDuration(snapped);
    }

    private void SetDifficulty(int index)
    {
        currentDifficulty = Mathf.Clamp(index, 0, Presets.Length - 1);
        ApplyPreset(currentDifficulty);
        UpdateDifficultyButtons();
        UpdateLabels();
    }

    private void UpdateDifficultyButtons()
    {
        if (difficultyButtons == null) return;
        for (int i = 0; i < difficultyButtons.Length; i++)
            if (difficultyButtons[i] != null)
                difficultyButtons[i].interactable = (i != currentDifficulty);
    }

    private void ApplyPreset(int index)
    {
        var preset = Presets[index];
        config.gapRatio   = preset.gapRatio;
        config.lockRatio  = preset.lockRatio;
        config.extraTiles = preset.extraTiles;
        // reserveSlots = tiles que el jugador tendra que colocar
        // = aproximadamente gapRatio * 70% del total de celdas (el camino cubre el 70%)
        int totalCells      = config.columns * config.rows;
        config.reserveSlots = Mathf.Max(2, Mathf.CeilToInt(totalCells * 0.7f * preset.gapRatio));
    }

    private void UpdateLabels()
    {
        if (gridSizeLabel != null)
            gridSizeLabel.text = $"{config.columns}x{config.rows}";
        if (durationLabel != null)
            durationLabel.text = FormatDuration(config.matchDuration);
    }

    private string FormatDuration(float seconds)
    {
        int min = (int)seconds / 60;
        int sec = (int)seconds % 60;
        return min > 0 ? $"{min}m {sec:00}s" : $"{sec}s";
    }
}
