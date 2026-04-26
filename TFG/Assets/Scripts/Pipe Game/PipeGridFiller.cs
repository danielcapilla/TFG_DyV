using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public class PipeGridFiller : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject generatorPrefab;
    [SerializeField] private GameObject receiverPrefab;
    [SerializeField] private GameObject straightTilePrefab;
    [SerializeField] private GameObject curveTilePrefab;
    [SerializeField] private GameObject tSplitTilePrefab;
    [SerializeField] private GameObject crossTilePrefab;

    [Header("Configuracion")]
    [SerializeField] private int seed = 0;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField][Range(0.1f, 0.8f)] private float gapRatio  = 0.35f;
    [SerializeField][Range(0f,   1f)]   private float lockRatio = 0.4f;
    [SerializeField][Range(0, 10)]      private int   extraTiles = 3;

    private GridGenerator grid;
    private List<GameObject> spawned = new List<GameObject>();
    private PipeGenerator spawnedGenerator;
    private PipeReceiver  spawnedReceiver;

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    private void Awake() => grid = GetComponent<GridGenerator>();

    private void Start()
    {
        if (IsOffline) Fill();
        // Online: PipeSeedSync se encarga de llamar Fill con el seed correcto
    }



    public int GetSeed() => useRandomSeed ? Random.Range(1, 999999) : seed;
    public void Fill() => Fill(GetSeed());

    public void Fill(int usedSeed)
    {
        if (grid == null) grid = GetComponent<GridGenerator>();
        ClearSpawned();
        if (grid.Slots == null) { Debug.LogWarning("PipeGridFiller: Slots es null."); return; }

        // Suprimir evaluaciones del checker durante el spawn
        var checkerForSuppress = GetComponent<PipeConnectionChecker>() ?? FindFirstObjectByType<PipeConnectionChecker>();
        if (checkerForSuppress != null) checkerForSuppress.SuppressEvaluate = true;

        int cols = grid.Columns;
        int rows = grid.Rows;

        Random.InitState(usedSeed);

        // 1. Buscar camino valido hasta 20 intentos
        Vector2Int genCell = default, recCell = default, pathStart = default, pathEnd = default;
        TileDirection genOutDir = default, recInDir = default;
        List<Vector2Int> path = null;

        for (int attempt = 0; attempt < 20 && path == null; attempt++)
        {
            int border    = Random.Range(0, 4);
            int oppBorder = (border + 2) % 4;

            genCell   = RandomBorderCell(border,    cols, rows);
            recCell   = RandomBorderCell(oppBorder, cols, rows);
            genOutDir = TileShapeData.Opposite(BorderOutDir(border));
            recInDir  = TileShapeData.Opposite(BorderOutDir(oppBorder));
            pathStart = genCell + DirToOffset(genOutDir);
            pathEnd   = recCell + DirToOffset(recInDir);

            if (pathStart.x < 0 || pathStart.x >= cols || pathStart.y < 0 || pathStart.y >= rows ||
                pathEnd.x   < 0 || pathEnd.x   >= cols || pathEnd.y   < 0 || pathEnd.y   >= rows)
                continue;

            if (Mathf.Abs(genCell.x - recCell.x) + Mathf.Abs(genCell.y - recCell.y) < 2)
                continue;

            path = GeneratePath(pathStart, pathEnd, cols, rows, genCell, recCell);
        }

        if (path == null) { Debug.LogWarning("PipeGridFiller: no se pudo generar camino."); return; }

        // 2. Generator y receiver
        var checker = GetComponent<PipeConnectionChecker>() ?? FindFirstObjectByType<PipeConnectionChecker>();

        SpawnInSlot(generatorPrefab, genCell, go => {
            var gen = go.GetComponent<PipeGenerator>();
            if (gen == null) return;
            SetField(gen, "outputDirection", genOutDir);
            spawnedGenerator = gen;
            if (checker != null) checker.generator = gen;
        });

        SpawnInSlot(receiverPrefab, recCell, go => {
            var rec = go.GetComponent<PipeReceiver>();
            if (rec == null) return;
            SetField(rec, "inputDirection", recInDir);
            spawnedReceiver = rec;
            if (checker != null) checker.receiver = rec;
        });

        // 3. Huecos
        HashSet<int> gaps = ChooseGaps(path.Count);

        // 4. Tiles del camino
        var used = new HashSet<Vector2Int> { genCell, recCell, pathStart, pathEnd };
        int reserveIndex = 0;
        TileSlot[] reserve = grid.ReserveSlots;

        // Probabilidad de bloqueo inversamente proporcional a gapRatio
        // (mas huecos = menos bloqueadas, para no hacer el puzzle imposible)
        float effectiveLockRatio = lockRatio * (1f - gapRatio);

        for (int i = 0; i < path.Count; i++)
        {
            TileDirection entry = i == 0
                ? TileShapeData.Opposite(genOutDir)
                : TileShapeData.Opposite(DirBetween(path[i-1], path[i]));

            TileDirection exit = i == path.Count - 1
                ? TileShapeData.Opposite(recInDir)
                : DirBetween(path[i], path[i+1]);

            TileDirection openings = entry | exit;

            if (gaps.Contains(i))
            {
                if (reserve != null && reserveIndex < reserve.Length)
                    SpawnTileInReserve(reserve[reserveIndex++], openings);
            }
            else
            {
                // Adyacentes a generator (i==0) y receiver (i==Count-1): siempre bloqueadas
                bool locked = (i == 0 || i == path.Count - 1) || Random.value < effectiveLockRatio;
                SpawnTileInSlot(path[i], openings, locked);
                used.Add(path[i]);
            }
        }

        // 5. Tiles extra
        SpawnExtra(used, cols, rows);

        // Reactivar evaluacion al terminar el spawn
        if (checkerForSuppress != null) checkerForSuppress.SuppressEvaluate = false;

        Debug.Log($"[Filler] seed={usedSeed} gen=[{genCell}] rec=[{recCell}] path={path.Count} gaps={gaps.Count}");
    }

    // ── Borde ─────────────────────────────────────────────────────────────────

    private Vector2Int RandomBorderCell(int border, int cols, int rows)
    {
        switch (border)
        {
            case 0: return new Vector2Int(Random.Range(0, cols), 0);
            case 1: return new Vector2Int(Random.Range(0, cols), rows - 1);
            case 2: return new Vector2Int(0, Random.Range(0, rows));
            default: return new Vector2Int(cols - 1, Random.Range(0, rows));
        }
    }

    private TileDirection BorderOutDir(int border)
    {
        switch (border)
        {
            case 0: return TileDirection.South;
            case 1: return TileDirection.North;
            case 2: return TileDirection.West;
            default: return TileDirection.East;
        }
    }

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

    // ── Camino ────────────────────────────────────────────────────────────────

    private List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int target,
        int cols, int rows, Vector2Int blocked1 = default, Vector2Int blocked2 = default)
    {
        var blocked  = new HashSet<Vector2Int> { blocked1, blocked2 };
        int minCells = Mathf.RoundToInt((cols * rows - blocked.Count) * 0.7f);
        var path     = new List<Vector2Int> { start };
        var visited  = new HashSet<Vector2Int>(blocked) { start };
        return DFS(start, target, cols, rows, blocked, path, visited, minCells) ? path : null;
    }

    private bool DFS(Vector2Int cur, Vector2Int target, int cols, int rows,
        HashSet<Vector2Int> blocked, List<Vector2Int> path, HashSet<Vector2Int> visited, int minCells)
    {
        if (cur == target && path.Count >= minCells) return true;

        var dirs = new[] {
            new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,0), new Vector2Int(-1,0)
        };
        for (int i = dirs.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = dirs[i]; dirs[i] = dirs[j]; dirs[j] = tmp;
        }

        foreach (var d in dirs)
        {
            var next = cur + d;
            if (next.x < 0 || next.x >= cols || next.y < 0 || next.y >= rows) continue;
            if (visited.Contains(next)) continue;
            path.Add(next); visited.Add(next);
            if (DFS(next, target, cols, rows, blocked, path, visited, minCells)) return true;
            path.RemoveAt(path.Count - 1); visited.Remove(next);
        }

        if (cur == target && path.Count >= minCells) return true;
        return false;
    }

    // ── Huecos ────────────────────────────────────────────────────────────────

    private HashSet<int> ChooseGaps(int pathCount)
    {
        var gaps = new HashSet<int>();
        int gapCount = Mathf.RoundToInt((pathCount - 2) * gapRatio);
        var candidates = new List<int>();
        for (int i = 1; i < pathCount - 1; i++) candidates.Add(i);
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = candidates[i]; candidates[i] = candidates[j]; candidates[j] = tmp;
        }
        int added = 0;
        foreach (int idx in candidates)
        {
            if (added >= gapCount) break;
            if (gaps.Contains(idx - 1) || gaps.Contains(idx + 1)) continue;
            gaps.Add(idx); added++;
        }
        return gaps;
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────

    private void ClearSpawned()
    {
        foreach (var go in spawned)
        {
            if (go == null) continue;
            if (IsOffline) DestroyImmediate(go);
            else Destroy(go);
        }
        spawned.Clear();
        spawnedGenerator = null;
        spawnedReceiver  = null;
    }

    private void SpawnInSlot(GameObject prefab, Vector2Int cell, System.Action<GameObject> setup)
    {
        if (prefab == null) return;
        TileSlot slot = grid.Slots[cell.x, cell.y];
        if (slot == null) return;
        GameObject go = Instantiate(prefab);
        PlayerCarry.SetParentSafe(go, slot.transform);
        go.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        go.transform.localRotation = Quaternion.identity;
        setup(go);
        spawned.Add(go);
    }

    private void SpawnTileInSlot(Vector2Int cell, TileDirection openings, bool locked = false)
    {
        TileSlot slot = grid.Slots[cell.x, cell.y];
        if (slot == null) return;
        GameObject prefab = BestPrefab(openings, out int rotation);
        if (prefab == null) return;

        bool wasActive = prefab.activeSelf;
        prefab.SetActive(false);
        GameObject go = Instantiate(prefab);
        prefab.SetActive(wasActive);
        PlayerCarry.SetParentSafe(go, slot.transform);
        go.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        go.transform.localRotation = Quaternion.Euler(0, rotation, 0);
        go.SetActive(true);

        var tile = go.GetComponent<PipeTile>();
        if (tile != null)
        {
            for (int r = 0; r < rotation / 90; r++) tile.Rotate90();
            if (locked) tile.SetLocked(true);
        }

        var carryable = go.GetComponent<ICarryObject>();
        if (carryable != null) slot.ForcePlace(carryable);
        spawned.Add(go);
    }

    private void SpawnTileInReserve(TileSlot slot, TileDirection openings)
    {
        if (slot == null) return;
        GameObject prefab = BestPrefab(openings, out int rotation);
        if (prefab == null) return;

        bool wasActive = prefab.activeSelf;
        prefab.SetActive(false);
        GameObject go = Instantiate(prefab);
        prefab.SetActive(wasActive);
        PlayerCarry.SetParentSafe(go, slot.transform);
        go.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        go.transform.localRotation = Quaternion.Euler(0, rotation, 0);
        go.SetActive(true);

        var tile = go.GetComponent<PipeTile>();
        if (tile != null)
            for (int r = 0; r < rotation / 90; r++) tile.Rotate90();

        var carryable = go.GetComponent<ICarryObject>();
        if (carryable != null) slot.ForcePlace(carryable);
        spawned.Add(go);
    }

    private void SpawnExtra(HashSet<Vector2Int> used, int cols, int rows)
    {
        var free = new List<Vector2Int>();
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
                if (!used.Contains(new Vector2Int(c, r))) free.Add(new Vector2Int(c, r));

        for (int i = free.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = free[i]; free[i] = free[j]; free[j] = tmp;
        }

        var shapes = new[] {
            TileDirection.North | TileDirection.South,
            TileDirection.North | TileDirection.East,
            TileDirection.North | TileDirection.East | TileDirection.West,
        };

        int count = Mathf.Min(extraTiles, free.Count);
        for (int i = 0; i < count; i++)
        {
            var op = TileShapeData.Rotate(shapes[Random.Range(0, shapes.Length)], Random.Range(0, 4) * 90);
            SpawnTileInSlot(free[i], op, false);
        }
    }

    // ── Utilidades ────────────────────────────────────────────────────────────

    private TileDirection DirBetween(Vector2Int from, Vector2Int to)
    {
        var d = to - from;
        if (d == new Vector2Int( 0,  1)) return TileDirection.North;
        if (d == new Vector2Int( 0, -1)) return TileDirection.South;
        if (d == new Vector2Int( 1,  0)) return TileDirection.East;
        if (d == new Vector2Int(-1,  0)) return TileDirection.West;
        return TileDirection.None;
    }

    private GameObject BestPrefab(TileDirection openings, out int rotation)
    {
        rotation = 0;
        int bits = BitCount((int)openings);
        if (bits >= 4 && crossTilePrefab) return crossTilePrefab;

        var shapes = bits == 3
            ? new[] { TileShapeType.TSplit }
            : new[] { TileShapeType.Straight, TileShapeType.Curve };

        foreach (var shape in shapes)
        {
            var baseOp = TileShapeData.GetOpenings(shape);
            for (int r = 0; r < 4; r++)
            {
                if (TileShapeData.Rotate(baseOp, r * 90) == openings)
                {
                    rotation = r * 90;
                    switch (shape)
                    {
                        case TileShapeType.Straight: return straightTilePrefab;
                        case TileShapeType.Curve:    return curveTilePrefab;
                        case TileShapeType.TSplit:   return tSplitTilePrefab;
                    }
                }
            }
        }
        return crossTilePrefab;
    }

    private void SetField(object obj, string fieldName, object value)
    {
        var f = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        f?.SetValue(obj, value);
    }

    private int BitCount(int v) { int c = 0; while (v != 0) { c += v & 1; v >>= 1; } return c; }
}
