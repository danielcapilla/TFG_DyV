using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PipeInfoManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private FiltersBehaviour filters;

    [Header("Grid Izquierdo (estado inicial)")]
    [SerializeField] private RectTransform initialGridRoot;

    [Header("Grid Derecho (intentos)")]
    [SerializeField] private RectTransform attemptGridRoot;

    [Header("UI Textos")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI attemptText;

    [Header("Navegacion Rondas")]
    [SerializeField] private Button prevRoundButton;
    [SerializeField] private Button nextRoundButton;

    [Header("Navegacion Intentos")]
    [SerializeField] private Button prevAttemptButton;
    [SerializeField] private Button nextAttemptButton;

    [Header("Indicador de exito")]
    [Tooltip("Imagen verde detras del grid derecho que se activa cuando el intento fue exitoso")]
    [SerializeField] private GameObject successIndicator;

    [Header("Sprites tiles normales")]
    [SerializeField] private Sprite straightSprite;
    [SerializeField] private Sprite curveSprite;
    [SerializeField] private Sprite tSplitSprite;
    [SerializeField] private Sprite crossSprite;
    [SerializeField] private Sprite generatorSprite;
    [SerializeField] private Sprite receiverSprite;
    [SerializeField] private Sprite emptySprite;

    [Header("Offset de rotacion base por tipo (grados, ajustar si el sprite base esta mal orientado)")]
    [SerializeField] private float straightRotationOffset = 0f;
    [SerializeField] private float curveRotationOffset    = 0f;
    [SerializeField] private float tSplitRotationOffset   = 0f;
    [SerializeField] private float crossRotationOffset    = 0f;
    [SerializeField] private float generatorRotationOffset = 0f;
    [SerializeField] private float receiverRotationOffset  = 0f;

    [Header("Sprites tiles bloqueadas")]
    [SerializeField] private Sprite lockedStraightSprite;
    [SerializeField] private Sprite lockedCurveSprite;
    [SerializeField] private Sprite lockedTSplitSprite;
    [SerializeField] private Sprite lockedCrossSprite;

    private PipeMatchData matchData;
    private int currentRound   = 0;
    private int currentAttempt = -1;

    private readonly List<GameObject> initialCells = new();
    private readonly List<GameObject> attemptCells = new();

    private void Awake()
    {
        if (prevRoundButton)   prevRoundButton  .onClick.AddListener(PrevRound);
        if (nextRoundButton)   nextRoundButton  .onClick.AddListener(NextRound);
        if (prevAttemptButton) prevAttemptButton.onClick.AddListener(PrevAttempt);
        if (nextAttemptButton) nextAttemptButton.onClick.AddListener(NextAttempt);
    }

    private void OnEnable()
    {
        matchData      = filters?.pipeMatch;
        currentRound   = 0;
        currentAttempt = -1;
        if (successIndicator != null) successIndicator.SetActive(false);
        Refresh();
    }

    private void OnDisable()
    {
        ClearGrid(initialCells);
        ClearGrid(attemptCells);
    }

    // ── Navegacion ────────────────────────────────────────────────────────────

    private void PrevRound()
    {
        if (matchData == null) return;
        currentRound   = Mathf.Max(0, currentRound - 1);
        currentAttempt = -1;
        Refresh();
    }

    private void NextRound()
    {
        if (matchData == null) return;
        currentRound   = Mathf.Min(matchData.rounds.Count - 1, currentRound + 1);
        currentAttempt = -1;
        Refresh();
    }

    private void PrevAttempt()
    {
        currentAttempt = Mathf.Max(-1, currentAttempt - 1);
        Refresh();
    }

    private void NextAttempt()
    {
        if (matchData == null) return;
        var round = matchData.rounds[currentRound];
        currentAttempt = Mathf.Min(round.attempts.Count - 1, currentAttempt + 1);
        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    private void Refresh()
    {
        ClearGrid(initialCells);
        ClearGrid(attemptCells);

        if (matchData == null || matchData.rounds.Count == 0) return;

        var round = matchData.rounds[currentRound];

        // Texto ronda
        if (roundText)
            roundText.text = $"Ronda {currentRound + 1} / {matchData.rounds.Count}  |  {round.timeSeconds:F1}s";

        // Botones ronda
        if (prevRoundButton) prevRoundButton.interactable = currentRound > 0;
        if (nextRoundButton) nextRoundButton.interactable = currentRound < matchData.rounds.Count - 1;

        // Grid izquierdo: estado inicial (siempre fijo)
        DrawGrid(initialGridRoot, initialCells, round.initialState);

        // Grid derecho: estado inicial o intento seleccionado
        PipeGridSnapshot snapshot;
        string info;
        if (currentAttempt < 0)
        {
            snapshot = round.initialState;
            info     = "Estado inicial";
        }
        else
        {
            var attempt = round.attempts[currentAttempt];
            snapshot    = attempt.gridState;
            info        = $"Intento {currentAttempt + 1} / {round.attempts.Count}";
        }

        if (attemptText) attemptText.text = info;

        if (prevAttemptButton) prevAttemptButton.interactable = currentAttempt >= 0;
        if (nextAttemptButton) nextAttemptButton.interactable =
            round.attempts.Count > 0 && currentAttempt < round.attempts.Count - 1;

        // Activar contorno verde si el intento fue exitoso
        if (successIndicator != null)
        {
            bool isSuccess = currentAttempt >= 0 && round.attempts[currentAttempt].success;
            successIndicator.SetActive(isSuccess);
        }

        DrawGrid(attemptGridRoot, attemptCells, snapshot);
    }

    // ── Dibujar grid ──────────────────────────────────────────────────────────

    private void DrawGrid(RectTransform root, List<GameObject> cells, PipeGridSnapshot snapshot)
    {
        if (root == null || snapshot == null) return;

        int cols = matchData.gridColumns;
        int rows = matchData.gridRows;

        var layout = root.GetComponent<GridLayoutGroup>();
        if (layout != null)
        {
            layout.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = cols;
            float w = (root.rect.width  - (cols - 1) * layout.spacing.x - layout.padding.left - layout.padding.right)  / cols;
            float h = (root.rect.height - (rows - 1) * layout.spacing.y - layout.padding.top  - layout.padding.bottom) / rows;
            float c = Mathf.Floor(Mathf.Min(w, h));
            layout.cellSize = new Vector2(c, c);
        }

        // Mapa rapido col,row -> tile
        var tileMap = new Dictionary<(int, int), PipeTileState>();
        foreach (var t in snapshot.tiles)
            tileMap[(t.col, t.row)] = t;

        // Dibujar de arriba a abajo (row descendente)
        for (int row = rows - 1; row >= 0; row--)
        {
            for (int col = 0; col < cols; col++)
            {
                tileMap.TryGetValue((col, row), out PipeTileState tileState);
                var go = CreateCell(root, tileState);
                cells.Add(go);
            }
        }
    }

    private GameObject CreateCell(RectTransform root, PipeTileState tileState)
    {
        var go  = new GameObject("Cell", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(root, false);
        var img = go.GetComponent<Image>();

        if (tileState == null)
        {
            img.sprite = emptySprite;
            img.color  = emptySprite != null ? Color.white : new Color(1, 1, 1, 0.1f);
            return go;
        }

        img.sprite = GetSprite(tileState);
        img.color  = Color.white;
        float rot2D = GetRotation2D(tileState);
        go.transform.localRotation = Quaternion.Euler(0, 0, rot2D);
        return go;
    }

    // Convierte la rotacion 3D (Y-axis horario) a rotacion 2D (Z-axis)
    // En 3D: horario visto desde arriba. En UI: positivo = antihorario.
    // North en sprite = arriba en pantalla = 0 grados en UI
    private float GetRotation2D(PipeTileState t)
    {
        float offset = 0f;
        switch (t.shapeType)
        {
            case "Straight":  offset = straightRotationOffset;  break;
            case "Curve":     offset = curveRotationOffset;     break;
            case "TSplit":    offset = tSplitRotationOffset;    break;
            case "Cross":     offset = crossRotationOffset;     break;
            case "Generator": offset = generatorRotationOffset; break;
            case "Receiver":  offset = receiverRotationOffset;  break;
        }
        // Rotacion 3D horario -> UI antihorario: negar el angulo
        return -t.rotation + offset;
    }

    private Sprite GetSprite(PipeTileState t)
    {
        switch (t.shapeType)
        {
            case "Straight":  return t.isLocked && lockedStraightSprite ? lockedStraightSprite : straightSprite;
            case "Curve":     return t.isLocked && lockedCurveSprite    ? lockedCurveSprite    : curveSprite;
            case "TSplit":    return t.isLocked && lockedTSplitSprite   ? lockedTSplitSprite   : tSplitSprite;
            case "Cross":     return t.isLocked && lockedCrossSprite    ? lockedCrossSprite    : crossSprite;
            case "Generator": return generatorSprite;
            case "Receiver":  return receiverSprite;
            default:           return emptySprite;
        }
    }

    private void ClearGrid(List<GameObject> cells)
    {
        foreach (var go in cells) if (go) Destroy(go);
        cells.Clear();
    }
}
