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
    [SerializeField] private bool randomSeed = true;
    [SerializeField][Range(0.0f, 0.8f)] private float gapRatio = 0.35f;
    [SerializeField][Range(0, 10)]      private int extraTiles  = 3;

    private GridGenerator grid;
    private List<GameObject> spawned = new List<GameObject>();
    private PipeGenerator spawnedGenerator;
    private PipeReceiver  spawnedReceiver;

    private void Awake() => grid = GetComponent<GridGenerator>();
    private void Start() => Fill();

    public void Fill()
    {
        if (grid == null) grid = GetComponent<GridGenerator>();
        ClearSpawned();

        // Esperar a que el grid tenga su matriz lista
        if (grid.Slots == null) { Debug.LogWarning("PipeGridFiller: Slots es null."); return; }

        int cols = grid.Columns;
        int rows = grid.Rows;

        int usedSeed = randomSeed ? Random.Range(0, 999999) : seed;
        Random.InitState(usedSeed);

        // Intentar hasta 20 veces con distintas celdas de borde hasta encontrar un camino valido
        Vector2Int genCell = default, recCell = default, pathStart = default, pathEnd = default;
        TileDirection genOutDir = default, recInDir = default;
        List<Vector2Int> path = null;
        int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts && path == null; attempt++)
        {
            int border    = Random.Range(0, 4);
            int oppBorder = (border + 2) % 4;

            genCell    = RandomBorderCell(border,    cols, rows);
            recCell    = RandomBorderCell(oppBorder, cols, rows);
            genOutDir  = TileShapeData.Opposite(BorderOutDir(border));
            recInDir   = TileShapeData.Opposite(BorderOutDir(oppBorder));
            pathStart  = genCell + DirToOffset(genOutDir);
            pathEnd    = recCell + DirToOffset(recInDir);

            // Verificar que pathStart y pathEnd esten dentro del grid
            if (pathStart.x < 0 || pathStart.x >= cols || pathStart.y < 0 || pathStart.y >= rows ||
                pathEnd.x   < 0 || pathEnd.x   >= cols || pathEnd.y   < 0 || pathEnd.y   >= rows)
                continue;

            // Evitar que gen y rec esten demasiado cerca (distancia minima de 2)
            if (Mathf.Abs(genCell.x - recCell.x) + Mathf.Abs(genCell.y - recCell.y) < 2)
                continue;

            path = GeneratePath(pathStart, pathEnd, cols, rows, genCell, recCell);
        }

        if (path == null) { Debug.LogWarning("PipeGridFiller: no se pudo generar camino tras 20 intentos."); return; }

        // 3. Spawnear generator y receiver en sus slots
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

        // 4. Huecos
        HashSet<int> gaps = ChooseGaps(path.Count);

        // 5. Tiles del camino
        var used = new HashSet<Vector2Int> { genCell, recCell, pathStart, pathEnd };
        for (int i = 0; i < path.Count; i++)
        {
            if (gaps.Contains(i)) continue;

            // entry: de donde viene la señal al entrar a esta celda
            TileDirection entry = i == 0
                ? TileShapeData.Opposite(genOutDir)        // viene del generator
                : TileShapeData.Opposite(DirBetween(path[i-1], path[i])); // viene del anterior

            // exit: hacia donde sale la señal de esta celda
            TileDirection exit = i == path.Count - 1
                ? TileShapeData.Opposite(recInDir)         // sale hacia el receiver
                : DirBetween(path[i], path[i+1]);          // sale hacia el siguiente

            SpawnTileInSlot(path[i], entry | exit);
            used.Add(path[i]);
        }

        // 6. Tiles extra en celdas libres
        SpawnExtra(used, cols, rows);


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

    // ── Camino ────────────────────────────────────────────────────────────────

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

    private List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int target, int cols, int rows, Vector2Int blocked1 = default, Vector2Int blocked2 = default)
    {
        var blocked  = new HashSet<Vector2Int> { blocked1, blocked2 };
        int total    = cols * rows - blocked.Count;
        // Exigir que el camino cubra al menos el 70% del grid
        int minCells = Mathf.RoundToInt(total * 0.7f);

        var path    = new List<Vector2Int> { start };
        var visited = new HashSet<Vector2Int>(blocked) { start };

        if (DFS(start, target, cols, rows, blocked, path, visited, minCells))
            return path;
        return null;
    }

    private bool DFS(Vector2Int current, Vector2Int target, int cols, int rows,
        HashSet<Vector2Int> blocked, List<Vector2Int> path, HashSet<Vector2Int> visited, int minCells)
    {
        // Solo permitir llegar al objetivo si ya hemos visitado suficientes celdas
        if (current == target && path.Count >= minCells)
            return true;

        var dirs = new Vector2Int[]
        {
            new Vector2Int( 0,  1),
            new Vector2Int( 0, -1),
            new Vector2Int( 1,  0),
            new Vector2Int(-1,  0),
        };
        // Shuffle completo
        for (int i = dirs.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = dirs[i]; dirs[i] = dirs[j]; dirs[j] = tmp;
        }

        foreach (var d in dirs)
        {
            var next = current + d;
            if (next.x < 0 || next.x >= cols || next.y < 0 || next.y >= rows) continue;
            if (visited.Contains(next)) continue;

            path.Add(next);
            visited.Add(next);

            if (DFS(next, target, cols, rows, blocked, path, visited, minCells))
                return true;

            path.RemoveAt(path.Count - 1);
            visited.Remove(next);
        }

        // Si no hay mas vecinos y estamos en el target con suficientes celdas, aceptar
        if (current == target && path.Count >= minCells)
            return true;

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
        foreach (var go in spawned) if (go != null) DestroyImmediate(go);
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

    private void SpawnTileInSlot(Vector2Int cell, TileDirection openings)
    {
        TileSlot slot = grid.Slots[cell.x, cell.y];
        if (slot == null) return;
        GameObject prefab = BestPrefab(openings, out int rotation);
        if (prefab == null) return;

        GameObject go = Instantiate(prefab);
        PlayerCarry.SetParentSafe(go, slot.transform);
        go.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        go.transform.localRotation = Quaternion.Euler(0, rotation, 0);

        // Sincronizar offlineRotation
        var tile = go.GetComponent<PipeTile>();
        if (tile != null)
            for (int r = 0; r < rotation / 90; r++)
                tile.Rotate90();

        // Registrar en slot para que el sistema de carry lo reconozca
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

        var shapes = new TileDirection[] {
            TileDirection.North | TileDirection.South,
            TileDirection.North | TileDirection.East,
            TileDirection.North | TileDirection.East | TileDirection.West,
        };

        int count = Mathf.Min(extraTiles, free.Count);
        for (int i = 0; i < count; i++)
        {
            var op = TileShapeData.Rotate(shapes[Random.Range(0, shapes.Length)], Random.Range(0, 4) * 90);
            SpawnTileInSlot(free[i], op);
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
