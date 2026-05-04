using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Graba los datos de una partida de Pipe Game.
/// Se suscribe a PipeGridFiller (ronda nueva) y PipeLever (intento de palanca).
/// Al terminar la partida, entrega PipeMatchData a PipeDatabaseService.
/// </summary>
public class PipeMatchRecorder : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PipeGridFiller filler;
    [SerializeField] private PipeLever lever;
    [SerializeField] private GridGenerator grid;
    [SerializeField] private PipeConnectionChecker checker;
    [SerializeField] private PipeGameConfigSO config;
    [SerializeField] private PipeDatabaseService dbService;

    private PipeMatchData matchData;
    private PipeRound currentRound;
    private float roundStartTime;
    private int roundNumber = 0;
    private int attemptNumber = 0;
    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    private void Awake()
    {
        filler.OnFillCompleted  += OnNewRound;
        lever.OnLeverActivated  += OnLeverPulled;
        checker.OnCircuitCompleted += OnCircuitCompleted;
    }

    private void OnDestroy()
    {
        if (filler != null) filler.OnFillCompleted    -= OnNewRound;
        if (lever  != null) lever.OnLeverActivated    -= OnLeverPulled;
        if (checker != null) checker.OnCircuitCompleted -= OnCircuitCompleted;
    }

    public void StartRecording(string teacherCode, string classCode, string difficulty)
    {
        Debug.Log($"[Recorder] StartRecording tc={teacherCode} class={classCode} diff={difficulty}");
        matchData = new PipeMatchData
        {
            date          = System.DateTime.Today.ToString("yyyy-MM-dd"),
            teacherCode   = teacherCode,
            classCode     = classCode,
            gridColumns   = config != null ? config.columns : grid.Columns,
            gridRows      = config != null ? config.rows    : grid.Rows,
            difficulty    = difficulty,
            matchDuration = config != null ? config.matchDuration : 180f
        };
        roundNumber   = 0;
        attemptNumber = 0;
    }

    private void OnNewRound()
    {
        Debug.Log("[Recorder] OnNewRound matchData=" + (matchData != null));
        // Si StartRecording no se ha llamado aun, crear matchData con valores por defecto
        if (matchData == null)
        {
            matchData = new PipeMatchData
            {
                date          = System.DateTime.Today.ToString("yyyy-MM-dd"),
                teacherCode   = PlayerData.ClassCode,
                classCode     = PlayerData.ClassCode,
                gridColumns   = config != null ? config.columns : grid.Columns,
                gridRows      = config != null ? config.rows    : grid.Rows,
                difficulty    = config != null ? config.difficulty : "Normal",
                matchDuration = config != null ? config.matchDuration : 180f
            };
        }
        roundNumber++;
        attemptNumber = 0;
        roundStartTime = Time.time;

        currentRound = new PipeRound
        {
            roundNumber  = roundNumber,
            initialState = CaptureGrid()
        };

        matchData?.rounds.Add(currentRound);
    }

    private void OnLeverPulled()
    {
        if (currentRound == null) return;
        attemptNumber++;
        currentRound.attempts.Add(new PipeAttempt
        {
            attemptNumber = attemptNumber,
            success       = false,
            gridState     = CaptureGrid()
        });
    }

    private void OnCircuitCompleted()
    {
        if (currentRound == null) return;
        currentRound.timeSeconds = Time.time - roundStartTime;
        // Marcar el ultimo intento como exitoso
        if (currentRound.attempts.Count > 0)
            currentRound.attempts[currentRound.attempts.Count - 1].success = true;
    }

    public void SaveMatch(string teacherCode, string classCode, System.Action<int> onComplete = null)
    {
        Debug.Log($"[Recorder] SaveMatch matchData={matchData != null} dbService={dbService != null} rounds={matchData?.rounds?.Count}");
        if (matchData == null || dbService == null) { Debug.LogWarning("[Recorder] matchData o dbService es null!"); onComplete?.Invoke(1); return; }
        dbService.RegisterGame(matchData, onComplete);
    }

    // ── Captura del grid ──────────────────────────────────────────────────────

    // Convierte una TileDirection a grados en el sistema 3D (Y-axis)
    // North=0, East=90, South=180, West=270
    private int DirToAngle(TileDirection dir)
    {
        switch (dir)
        {
            case TileDirection.North: return 0;
            case TileDirection.East:  return 90;
            case TileDirection.South: return 180;
            case TileDirection.West:  return 270;
            default: return 0;
        }
    }

    private PipeGridSnapshot CaptureGrid()
    {
        var snapshot = new PipeGridSnapshot();
        var slots    = grid.Slots;
        if (slots == null) return snapshot;

        for (int col = 0; col < grid.Columns; col++)
        {
            for (int row = 0; row < grid.Rows; row++)
            {
                var slot = slots[col, row];
                if (slot == null) continue;

                // Comprobar si hay generator o receiver en este slot
                var gen = slot.GetComponentInChildren<PipeGenerator>();
                if (gen != null)
                {
                    snapshot.tiles.Add(new PipeTileState { col = col, row = row, shapeType = "Generator",
                        rotation = DirToAngle(gen.OutputDirection), isLocked = true });
                    continue;
                }
                var recv = slot.GetComponentInChildren<PipeReceiver>();
                if (recv != null)
                {
                    snapshot.tiles.Add(new PipeTileState { col = col, row = row, shapeType = "Receiver",
                        rotation = DirToAngle(recv.InputDirection), isLocked = true });
                    continue;
                }

                var tile = slot.PlacedTile;
                if (tile == null) continue;

                snapshot.tiles.Add(new PipeTileState
                {
                    col       = col,
                    row       = row,
                    shapeType = tile.ShapeType.ToString(),
                    rotation  = tile.CurrentRotation,
                    isLocked  = tile.IsLocked
                });
            }
        }
        return snapshot;
    }
}
