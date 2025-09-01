using System.Collections.Generic;
using UnityEngine;

public class GridLevelGenerator : MonoBehaviour
{
    [Header("Tamaño del mapa")]
    [Min(2)] public int width = 16;
    [Min(2)] public int height = 12;

    [Header("Opciones")]
    [Min(10)] public int minPathLength = 20;
    [Range(0f, 0.9f)] public float obstacleDensity = 0.25f;
    public Vector2Int start = new Vector2Int(0, 0);
    [Min(1)] public int maxGoalTries = 200;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject obstaclePrefab;
    public GameObject startPrefab;
    public GameObject goalPrefab;

    [Header("Celdas")]
    public float cellSize = 1f;
    public int seed = -1;
    [Tooltip("Regenerar al pulsar R (modo Play).")]
    public bool allowRuntimeRegenerate = true;

    // Estados del grid
    // 0 = libre, 1 = obstáculo, 2 = inicio, 3 = meta
    private int[,] grid;
    private Vector2Int goal;
    private Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    Quaternion markerRot = Quaternion.Euler(90f, 0f, 0f); 


    // Para limpiar instancias anteriores
    private readonly List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        // Solo si es servidor
        Generate();
        // Se envia la grid a los clientes
    }

    void Update()
    {
        // Construir nuevos obstaculos
        if (allowRuntimeRegenerate && Application.isPlaying && Input.GetKeyDown(KeyCode.R))
        {
            Generate();
        }
    }

    public void Generate()
    {
        // Semilla, se usa la hora del sistema para que sea distinta a cada ejecucion
        if (seed == -1) Random.InitState(System.Environment.TickCount);
        else Random.InitState(seed);

        // Validaciones del mapa
        width = Mathf.Max(2, width);
        height = Mathf.Max(2, height);
        // Salida dentro del espacio
        start.x = Mathf.Clamp(start.x, 0, width - 1);
        start.y = Mathf.Clamp(start.y, 0, height - 1);

        // Reset escenas previas
        ClearSpawned();

        grid = new int[width, height];

        // Creacion del mapa siguiendo las reglas

        // 1) Elegir goal y hacer una BSF
        Dictionary<Vector2Int, Vector2Int> bfsCameFrom;
        if (!TryPickGoal(out goal, out bfsCameFrom))
        {
            Debug.Log("No se pudo encontrar una meta que cumpla minPathLength. Se usó fallback.");
        }

        // 2) Reconstruir camino garantizado (esta vez se guarda)
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int t = goal;
        while (t != start)
        {
            path.Add(t);
            t = bfsCameFrom[t];
        }
        path.Add(start);
        path.Reverse();

        // 3) Marcar inicio y meta en la grid
        grid[start.x, start.y] = 2;
        grid[goal.x, goal.y] = 3;

        // 4) Colocar obstaculos aleatorios sin bloquear el camino garantizado
        // Usar un HashSet para acceso O(1)
        HashSet<Vector2Int> pathSet = new HashSet<Vector2Int>(path);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                // No poner obstaculo en inicio, meta o camino garantizado
                if (cell == start || cell == goal) continue;
                if (pathSet.Contains(cell)) continue; // proteger camino
                // Decidir aleatoriamente si poner obstaculo
                if (Random.value < obstacleDensity)
                    grid[x, y] = 1;
            }
        }

        // 5) Construir escena con prefabs
        BuildSceneFromGrid();
    }

    private bool TryPickGoal(out Vector2Int picked, out Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        picked = Vector2Int.zero;
        cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        // cola para BFS (FIFO)
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        // Record del camino de las celdas visitadas
        cameFrom[start] = start;
        // Barajar el orden de exploracion para dar variedad
        Vector2Int[] localDirs = (Vector2Int[])dirs.Clone();
        Shuffle(localDirs);

        while (queue.Count > 0)
        {
            // Sacar la antigua
            Vector2Int cur = queue.Dequeue();
            foreach (var dir in dirs)
            {
                // Nueva celda
                Vector2Int next = cur + dir;
                // Validar dentro de limites y no visitada
                if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height) continue;
                if (cameFrom.ContainsKey(next)) continue;
                // Añadir a visitadas y cola
                cameFrom[next] = cur;
                queue.Enqueue(next);
            }
        }

        // Recoger posibles candidatos que cumplan al menos minPathLength
        List<Vector2Int> candidates = new List<Vector2Int>();
        foreach (var c in cameFrom)
        {
            Vector2Int cell = c.Key;
            // Ignorar el start
            if (cell == start) continue;

            // Contar longitud real de pasos desde start
            int length = 0;
            Vector2Int t = cell;
            // Volver atras hasta el inicio
            while (t != start)
            {
                length++;
                t = cameFrom[t];
            }
            // Si cumple, añadir a candidatos
            if (length >= minPathLength)
                candidates.Add(cell);
        }

        // Fallback si no hay candidatos válidos
        if (candidates.Count == 0)
        {
            // Elegir el punto más alejado por Manhattan
            picked = FarthestByManhattan(start);
            return false;
        }

        // Elegir aleatoriamente una meta válida
        picked = candidates[Random.Range(0, candidates.Count)];
        return true;
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
        // cola para BFS (FIFO)
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(s);
        // Record del camino de las celdas visitadas
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[s] = s;

        // Barajar el orden de exploracion para dar variedad
        Vector2Int[] localDirs = (Vector2Int[])dirs.Clone();
        if (shuffleNeighbors) Shuffle(localDirs);

        while (q.Count > 0)
        {
            // Sacar la antigua
            var cur = q.Dequeue();
            // Si es la meta, terminar
            if (cur == g) break;
            // Explorar vecinos
            foreach (var d in localDirs)
            {
                // Nueva celda
                var nxt = cur + d;
                // Validar dentro de limites y no visitada
                if (nxt.x < 0 || nxt.y < 0 || nxt.x >= w || nxt.y >= h) continue;
                if (cameFrom.ContainsKey(nxt)) continue;
                // Añadir a visitadas y cola
                cameFrom[nxt] = cur;
                q.Enqueue(nxt);
            }
        }
        // Si no hemos llegado a la meta, no hay camino
        if (!cameFrom.ContainsKey(g)) return null;

        // Reconstruccion
        List<Vector2Int> path = new List<Vector2Int>();
        var t = g;
        // Volver atras hasta el inicio
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
        // Crear un padre para agrupar los elementos del nivel
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

                // Instanciar suelo si no es obstáculo, inicio o meta
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
                        rot = markerRot; // rotar para que quede plano
                        break;
                    case 3:
                        toSpawn = goalPrefab;
                        rot = markerRot; // rotar para que quede plano
                        break;
                }

                // Instanciar si hay prefab asignado
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
        // Eliminar instancias previas
        foreach (var go in spawned)
        {
            if (go != null)
            {
                // Diferenciar entre modo editor y play para destruir correctamente
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(go);
                else Destroy(go);
#else
                Destroy(go);
#endif
            }
        }
        spawned.Clear();

        // Tambien eliminar hijo previo "GridLevel" si quedo en jerarquia
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
    // Algoritmo de Fisher-Yates para barajar un array
    private void Shuffle(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int j = Random.Range(i, array.Length);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    // Gizmos para vista en editor 
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
