using System.Collections.Generic;
using UnityEngine;

public class GridLevelGenerator : MonoBehaviour
{
    [Header("Tamaño del mapa")]
    [Min(2)] public int width = 16;
    [Min(2)] public int height = 12;

    [Header("Reglas")]
    [Tooltip("Longitud mínima del camino Start->Goal (en celdas).")]
    [Min(1)] public int minPathLength = 10;
    [Range(0f, 0.9f)] public float obstacleDensity = 0.25f;
    [Tooltip("Posición fija del inicio (coordenadas de celda).")]
    public Vector2Int start = new Vector2Int(0, 0);
    [Tooltip("Intentos máximos para encontrar una meta válida.")]
    [Min(1)] public int maxGoalTries = 200;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject obstaclePrefab;
    public GameObject startPrefab;
    public GameObject goalPrefab;

    [Header("Opciones")]
    [Tooltip("Tamaño en unidades de cada celda (X/Z).")]
    public float cellSize = 1f;
    [Tooltip("Semilla aleatoria. -1 para aleatoria por tiempo.")]
    public int seed = -1;
    [Tooltip("Regenerar al pulsar R (modo Play).")]
    public bool allowRuntimeRegenerate = true;

    // Representación del grid
    // 0 = libre, 1 = obstáculo, 2 = inicio, 3 = meta
    private int[,] grid;
    private Vector2Int goal;
    private readonly Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    Quaternion floorRot = Quaternion.Euler(90f, 0f, 0f); // para quad como suelo
    Quaternion markerRot = Quaternion.Euler(90f, 0f, 0f); // para start/goal si son quads o planos


    // Para limpiar instancias anteriores
    private readonly List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        Generate();
    }

    void Update()
    {
        if (allowRuntimeRegenerate && Application.isPlaying && Input.GetKeyDown(KeyCode.R))
        {
            Generate();
        }
    }

    public void Generate()
    {
        // Semilla
        if (seed == -1) Random.InitState(System.Environment.TickCount);
        else Random.InitState(seed);

        // Validaciones
        width = Mathf.Max(2, width);
        height = Mathf.Max(2, height);
        start.x = Mathf.Clamp(start.x, 0, width - 1);
        start.y = Mathf.Clamp(start.y, 0, height - 1);

        // Reset escenas previas
        ClearSpawned();

        grid = new int[width, height];

        // 1) Seleccionar una meta variable con distancia mínima
        if (!TryPickGoal(out goal))
        {
            Debug.LogWarning("[GridLevelGenerator] No se pudo encontrar una meta que cumpla la distancia mínima. Disminuye minPathLength o aumenta el mapa.");
            // fallback: usar la posición más lejana por Manhattan
            goal = FarthestByManhattan(start);
        }

        // 2) Generar un camino garantizado (BFS con orden aleatorio de vecinos)
        var path = GeneratePathBFS(start, goal, width, height, shuffleNeighbors: true);
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("[GridLevelGenerator] No se encontró camino usando BFS (esto no debería ocurrir en grid vacío). Reintentando con meta fallback.");
            goal = FarthestByManhattan(start);
            path = GeneratePathBFS(start, goal, width, height, shuffleNeighbors: true);
        }

        // 3) Marcar inicio y meta
        grid[start.x, start.y] = 2;
        grid[goal.x, goal.y] = 3;

        // 4) Colocar obstáculos aleatorios SIN tocar el camino garantizado
        var pathSet = new HashSet<Vector2Int>(path);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var p = new Vector2Int(x, y);
                if (p == start || p == goal) continue;
                if (pathSet.Contains(p)) continue; // no bloquear el camino garantizado
                if (Random.value < obstacleDensity)
                {
                    grid[x, y] = 1;
                }
            }
        }

        // 5) Construir visualmente en escena
        BuildSceneFromGrid();
    }

    private bool TryPickGoal(out Vector2Int picked)
    {
        // Filtro rápido por Manhattan para respetar longitud mínima
        // Luego confirmamos con una BFS de longitud (en grid vacío)
        for (int i = 0; i < maxGoalTries; i++)
        {
            var candidate = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
            if (candidate == start) continue;

            int manhattan = Mathf.Abs(candidate.x - start.x) + Mathf.Abs(candidate.y - start.y);
            if (manhattan < minPathLength) continue;

            // En un grid vacío, la BFS más corta == Manhattan (con 4 direcciones).
            // No obstante, calculamos por robustez (y por si luego cambias dirs).
            var path = GeneratePathBFS(start, candidate, width, height, shuffleNeighbors: true);
            if (path != null && path.Count - 1 >= minPathLength)
            {
                picked = candidate;
                return true;
            }
        }

        picked = Vector2Int.zero;
        return false;
    }

    private Vector2Int FarthestByManhattan(Vector2Int from)
    {
        Vector2Int best = from;
        int bestDist = -1;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var p = new Vector2Int(x, y);
                if (p == from) continue;
                int d = Mathf.Abs(p.x - from.x) + Mathf.Abs(p.y - from.y);
                if (d > bestDist)
                {
                    bestDist = d;
                    best = p;
                }
            }
        }
        return best;
    }

    private List<Vector2Int> GeneratePathBFS(Vector2Int s, Vector2Int g, int w, int h, bool shuffleNeighbors)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(s);

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[s] = s;

        // Opcional: barajar el orden de exploración para variedad
        Vector2Int[] localDirs = (Vector2Int[])dirs.Clone();
        if (shuffleNeighbors) Shuffle(localDirs);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == g) break;

            foreach (var d in localDirs)
            {
                var nxt = cur + d;
                if (nxt.x < 0 || nxt.y < 0 || nxt.x >= w || nxt.y >= h) continue;
                if (cameFrom.ContainsKey(nxt)) continue;
                cameFrom[nxt] = cur;
                q.Enqueue(nxt);
            }
        }

        if (!cameFrom.ContainsKey(g)) return null;

        // Reconstrucción
        List<Vector2Int> path = new List<Vector2Int>();
        var t = g;
        while (t != s)
        {
            path.Add(t);
            t = cameFrom[t];
        }
        path.Add(s);
        path.Reverse();
        return path;
    }

    private void BuildSceneFromGrid()
    {
        // Crear un padre para agrupar
        var root = new GameObject("GridLevel");
        root.transform.SetParent(transform, false);
        spawned.Add(root);

        // Offset para centrar el mapa
        Vector3 origin = transform.position - new Vector3((width - 1) * 0.5f * cellSize, 0f, (height - 1) * 0.5f * cellSize);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = origin + new Vector3(x * cellSize, 0f, y * cellSize);

                // Piso (opcional)
                if (floorPrefab != null && grid[x, y] != 2 && grid[x, y] != 3)
                {
                    Quaternion floorRot = Quaternion.Euler(90f, 0f, 0f);
                    var floor = Instantiate(floorPrefab, worldPos, floorRot, root.transform);
                    spawned.Add(floor);
                }

                int val = grid[x, y];
                GameObject toSpawn = null;
                Quaternion rot = Quaternion.identity;

                switch (val)
                {
                    case 1:
                        toSpawn = obstaclePrefab;
                        rot = Quaternion.identity; // cubo no necesita rotación
                        break;
                    case 2:
                        toSpawn = startPrefab;
                        rot = markerRot; // si es un quad/plane
                        break;
                    case 3:
                        toSpawn = goalPrefab;
                        rot = markerRot; // idem
                        break;
                }

                if (toSpawn != null)
                {
                    var go = Instantiate(toSpawn, worldPos, rot, root.transform);
                    spawned.Add(go);
                }
            }
        }
    }

    private void ClearSpawned()
    {
        foreach (var go in spawned)
        {
            if (go != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(go);
                else Destroy(go);
#else
                Destroy(go);
#endif
            }
        }
        spawned.Clear();

        // También eliminar hijo previo "GridLevel" si quedó en jerarquía
        var child = transform.Find("GridLevel");
        if (child != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(child.gameObject);
            else Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }

    private void Shuffle(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int j = Random.Range(i, array.Length);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    // Gizmos para vista en editor (opcional, si no tienes prefabs)
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0f, 0f, 0.15f);
        Vector3 origin = transform.position - new Vector3((width - 1) * 0.5f * cellSize, 0f, (height - 1) * 0.5f * cellSize);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 p = origin + new Vector3(x * cellSize, 0f, y * cellSize);
                Gizmos.DrawWireCube(p + Vector3.up * 0.01f, new Vector3(cellSize * 0.95f, 0f, cellSize * 0.95f));
            }
        }
    }
#endif
}
