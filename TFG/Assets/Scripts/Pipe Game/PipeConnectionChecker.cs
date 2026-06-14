using Unity.Netcode;
using UnityEngine;

public class PipeConnectionChecker : NetworkBehaviour, IScoreEvent
{
    [Header("Referencias")]
    [SerializeField] public PipeGenerator generator;
    [SerializeField] public PipeReceiver  receiver;

    [Header("Sonidos")]
    [SerializeField] private AudioSource correctSound;
    [SerializeField] private AudioSource incorrectSound;

    private GridGenerator grid;
    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    private void Awake()
    {
        grid = GetComponent<GridGenerator>();
        if (grid == null) grid = FindFirstObjectByType<GridGenerator>();
    }

    public void SetReferences(PipeGenerator gen, PipeReceiver rec)
    {
        generator = gen;
        receiver  = rec;
        if (grid == null)
        {
            grid = GetComponent<GridGenerator>();
            if (grid == null) grid = FindFirstObjectByType<GridGenerator>();
        }
        Debug.LogWarning($"[Checker] SetReferences: gen={gen?.name} rec={rec?.name} grid={grid?.name}");
    }

    public void BuildSlotMap() { }

    public bool SuppressEvaluate { get; set; } = false;

    public event System.Action OnCircuitCompleted;
    public event System.Action<int> OnScorePoint;

    public void EvaluateCircuit()
    {
        if (SuppressEvaluate) return;
        if (!IsOffline && !IsServer) return;
        if (generator == null || receiver == null || grid == null || grid.Slots == null)
        {
            Debug.LogWarning($"[Checker] EvaluateCircuit bloqueado: gen={generator} rec={receiver} grid={grid} slots={grid?.Slots}");
            return;
        }

        bool connected = FindPath();
        generator.SetConnected(connected, IsOffline);
        receiver.SetConnected(connected, IsOffline);

        if (connected)
        {
            correctSound?.Play();
            OnCircuitCompleted?.Invoke();
            OnScorePoint?.Invoke(1);
        }
        else
        {
            incorrectSound?.Play();
        }

        Debug.Log($"[Checker] EvaluateCircuit: connected={connected}");
    }

    private bool FindPath()
    {
        var slots = grid.Slots;
        int cols  = grid.Columns;
        int rows  = grid.Rows;

        Vector2Int? genCell = FindCell(generator.gameObject);
        Vector2Int? recCell = FindCell(receiver.gameObject);
        if (genCell == null || recCell == null) return false;

        TileDirection outDir = generator.OutputDirection;
        TileDirection inDir  = receiver.InputDirection;

        Vector2Int start  = genCell.Value + DirToOffset(outDir);
        Vector2Int target = recCell.Value;

        if (!InBounds(start, cols, rows)) return false;

        var visited = new System.Collections.Generic.HashSet<(Vector2Int, TileDirection)>();
        var queue   = new System.Collections.Generic.Queue<(Vector2Int pos, TileDirection enteredFrom)>();
        queue.Enqueue((start, TileShapeData.Opposite(outDir)));

        while (queue.Count > 0)
        {
            var (pos, enteredFrom) = queue.Dequeue();
            var key = (pos, enteredFrom);
            if (visited.Contains(key)) continue;
            visited.Add(key);

            if (!InBounds(pos, cols, rows)) continue;
            PipeTile tile = slots[pos.x, pos.y]?.PlacedTile;
            if (tile == null) continue;
            if (!tile.HasOpening(enteredFrom)) continue;

            if (pos + DirToOffset(TileShapeData.Opposite(inDir)) == target
                && tile.HasOpening(TileShapeData.Opposite(inDir)))
                return true;

            foreach (TileDirection dir in new[]
                { TileDirection.North, TileDirection.South, TileDirection.East, TileDirection.West })
            {
                if (dir == enteredFrom) continue;
                if (!tile.HasOpening(dir)) continue;
                Vector2Int next = pos + DirToOffset(dir);
                if (!InBounds(next, cols, rows)) continue;
                if (!visited.Contains((next, TileShapeData.Opposite(dir))))
                    queue.Enqueue((next, TileShapeData.Opposite(dir)));
            }
        }
        return false;
    }

    private Vector2Int? FindCell(GameObject go)
    {
        if (go == null) return null;
        Transform slotT = go.transform.parent;
        if (slotT == null) return null;
        string name = slotT.name;
        int bracket = name.IndexOf('[');
        int comma   = name.IndexOf(',');
        int end     = name.IndexOf(']');
        if (bracket < 0 || comma < 0 || end < 0) return null;
        if (int.TryParse(name.Substring(bracket + 1, comma - bracket - 1), out int col) &&
            int.TryParse(name.Substring(comma + 1, end - comma - 1), out int row))
            return new Vector2Int(col, row);
        return null;
    }

    private bool InBounds(Vector2Int p, int cols, int rows)
        => p.x >= 0 && p.x < cols && p.y >= 0 && p.y < rows;

    private Vector2Int DirToOffset(TileDirection d)
    {
        switch (d)
        {
            case TileDirection.North: return new Vector2Int( 0,  1);
            case TileDirection.South: return new Vector2Int( 0, -1);
            case TileDirection.East:  return new Vector2Int( 1,  0);
            case TileDirection.West:  return new Vector2Int(-1,  0);
            default: return Vector2Int.zero;
        }
    }
}
