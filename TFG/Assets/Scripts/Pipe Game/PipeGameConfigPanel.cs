using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Rondas")]
    [SerializeField] private Slider maxRoundsSlider;
    [SerializeField] private TextMeshProUGUI maxRoundsLabel;

    [Header("Cerrar")]
    [SerializeField] private Button closeButton;

    private struct DifficultyPreset
    {
        public float gapRatio;
        public float lockRatio;
        public int   extraTiles;
        public DifficultyPreset(float gap, float lockR, int extra)
        { gapRatio = gap; lockRatio = lockR; extraTiles = extra; }
    }

    private static readonly string[] DifficultyNames = new[] { "Facil", "Normal", "Dificil", "Experto" };

    private static readonly DifficultyPreset[] Presets = new[]
    {
        new DifficultyPreset(0.50f, 0.10f, 5),
        new DifficultyPreset(0.35f, 0.35f, 3),
        new DifficultyPreset(0.25f, 0.60f, 1),
        new DifficultyPreset(0.40f, 0.80f, 0),
    };

    private PipeGameConfigSO config;
    private int currentDifficulty = 1;
    private Button[] difficultyButtons;

    public void Setup(GameConfigBaseSO baseConfig)
    {
        config = baseConfig as PipeGameConfigSO;
        if (config == null) return;

        if (gridSizeSlider != null)
        {
            gridSizeSlider.minValue     = 4;
            gridSizeSlider.maxValue     = 8;
            gridSizeSlider.wholeNumbers = true;
            gridSizeSlider.value        = Mathf.Max(config.columns, config.rows);
            gridSizeSlider.onValueChanged.AddListener(OnGridSizeChanged);
        }

        if (durationSlider != null)
        {
            durationSlider.minValue     = 60f;
            durationSlider.maxValue     = 331f;
            durationSlider.wholeNumbers = true;
            durationSlider.value        = config.infiniteTime ? 331f : config.matchDuration;
            durationSlider.onValueChanged.AddListener(OnDurationChanged);
        }

        if (maxRoundsSlider != null)
        {
            maxRoundsSlider.minValue     = 0;
            maxRoundsSlider.maxValue     = 20;
            maxRoundsSlider.wholeNumbers = true;
            maxRoundsSlider.value        = config.maxRounds;
            maxRoundsSlider.onValueChanged.AddListener(OnMaxRoundsChanged);
        }

        difficultyButtons = new[] { easyButton, normalButton, hardButton, expertButton };
        if (easyButton   != null) easyButton  .onClick.AddListener(() => SetDifficulty(0));
        if (normalButton != null) normalButton.onClick.AddListener(() => SetDifficulty(1));
        if (hardButton   != null) hardButton  .onClick.AddListener(() => SetDifficulty(2));
        if (expertButton != null) expertButton.onClick.AddListener(() => SetDifficulty(3));

        if (closeButton != null)
            closeButton.onClick.AddListener(() => MinigameSelectorBehaviour.Instance?.CloseConfigPanel());

        SetDifficulty(currentDifficulty);
        UpdateLabels();
    }

    private void OnGridSizeChanged(float value)
    {
        int size = Mathf.RoundToInt(value);
        config.columns = size;
        config.rows    = size;
        ApplyPreset(currentDifficulty);
        UpdateLabels();
    }

    private void OnDurationChanged(float value)
    {
        if (value > 330f)
        {
            config.infiniteTime  = true;
            config.matchDuration = 999999f;
            if (durationSlider != null) durationSlider.value = 331f;
            if (durationLabel  != null) durationLabel.text   = "\u221e";
        }
        else
        {
            config.infiniteTime  = false;
            float snapped        = Mathf.Round(value / 30f) * 30f;
            config.matchDuration = snapped;
            if (durationSlider != null) durationSlider.value = snapped;
            if (durationLabel  != null) durationLabel.text   = FormatDuration(snapped);
        }
    }

    private void OnMaxRoundsChanged(float value)
    {
        config.maxRounds = Mathf.RoundToInt(value);
        if (maxRoundsLabel != null)
            maxRoundsLabel.text = config.maxRounds == 0 ? "Sin limite" : config.maxRounds.ToString();
    }

    private void SetDifficulty(int index)
    {
        currentDifficulty = Mathf.Clamp(index, 0, Presets.Length - 1);
        if (config != null) config.difficulty = DifficultyNames[currentDifficulty];
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
        config.gapRatio     = preset.gapRatio;
        config.lockRatio    = preset.lockRatio;
        config.extraTiles   = preset.extraTiles;
        int totalCells      = config.columns * config.rows;
        config.reserveSlots = Mathf.Max(2, Mathf.CeilToInt(totalCells * 0.7f * preset.gapRatio));
    }

    private void UpdateLabels()
    {
        if (gridSizeLabel  != null) gridSizeLabel.text  = $"{config.columns}x{config.rows}";
        if (durationLabel  != null) durationLabel.text  = config.infiniteTime ? "\u221e" : FormatDuration(config.matchDuration);
        if (maxRoundsLabel != null) maxRoundsLabel.text = config.maxRounds == 0 ? "Sin limite" : config.maxRounds.ToString();
    }

    private string FormatDuration(float seconds)
    {
        int min = (int)seconds / 60;
        int sec = (int)seconds % 60;
        return min > 0 ? $"{min}m {sec:00}s" : $"{sec}s";
    }
}
