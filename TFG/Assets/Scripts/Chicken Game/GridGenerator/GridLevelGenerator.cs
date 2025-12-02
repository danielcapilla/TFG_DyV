using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridLevelGenerator : NetworkBehaviour
{
    [Header("Tamaño del mapa")]
    [Min(2)] public int width = 16;
    [Min(2)] public int height = 12;

    [Header("Opciones")]
    [Min(10)] public int minPathLength = 20;
    [Range(0f, 0.9f)] public float obstacleDensity = 0.25f;
    public Vector2Int start = new Vector2Int(0, 0);
    [Min(1)] public int maxGoalTries = 200;
    public bool tutorialGrid;   

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject[] obstaclePrefabs;
    public GameObject startPrefab;
    public GameObject goalPrefab;
    public GameObject[] wallPrefabs;


    [Header("Celdas")]
    public float cellSize = 1f;
    public int seed = -1;

    // Estados del grid
    // 0 = libre, 1 = obstáculo, 2 = inicio, 3 = meta
    private int[,] grid;
    private Vector2Int goal;
    private Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    Quaternion markerRot = Quaternion.Euler(90f, 0f, 0f); 


    // Para limpiar instancias anteriores
    private readonly List<GameObject> spawned = new List<GameObject>();

    private int[,] distanceMap; // guarda la distancia desde cada celda hasta la meta

    public static GridLevelGenerator Instance { get; private set; }

    public Action OnLevelGenerated;
    // Para los powerUps
    private List<Vector2Int> freeCells = new List<Vector2Int>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
           
        if(!IsServer) return;
        //Generate();
        //SendGridToClients();
    }
    public void GenerateLevel()
    {
        if (!IsServer) return;
        Generate();
        SendGridToClients();
    }
    public void GenerateTutorialLevel()
    {
        Generate();
        BuildSceneFromGrid();
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    private void Start()
    {
        if (tutorialGrid && !IsServer)
        {
            Generate();
            BuildSceneFromGrid();
        }
    }
    public void Generate()
    {
        // Semilla, se usa la hora del sistema para que sea distinta a cada ejecucion
        if (seed == -1) UnityEngine.Random.InitState(System.Environment.TickCount);
        else UnityEngine.Random.InitState(seed);

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
                if (pathSet.Contains(cell))
                {
                    // Guardar celda libre
                    freeCells.Add(cell);
                    continue; // proteger camino
                }
                
                // Decidir aleatoriamente si poner obstaculo
                if (UnityEngine.Random.value < obstacleDensity)
                    grid[x, y] = 1;
                // Guardar celda libre
                else
                {
                    freeCells.Add(cell);
                }
            }
        }
        ComputeDistanceMap();
        //GetFreeCells();
        // 5) Construir escena con prefabs
        //BuildSceneFromGrid();
        OnLevelGenerated?.Invoke();
        Debug.Log("Nivel generado: " + width + "x" + height + ", start: " + start + ", goal: " + goal);
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
            // Volver a barajar para mas aleatoriedad
            Shuffle(localDirs);
            foreach (var dir in localDirs)
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
        // Mas complejidad visual
        List<ScoredCandidate> scored = new List<ScoredCandidate>(candidates.Count);
        // Se calcula un score para cada candidato
        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int cand = candidates[i];
            float score = EvaluatePathComplexity(cand, cameFrom);
            scored.Add(new ScoredCandidate { pos = cand, score = score });
        }
        // Ordenar mayor a menor
        scored.Sort((a, b) => b.score.CompareTo(a.score));
        // Tomar top 30% para mantener aleatoriedad dentro los mas complejos
        int topCount = Mathf.Max(1, Mathf.CeilToInt(scored.Count * 0.3f));

        picked = candidates[UnityEngine.Random.Range(0, topCount)];
        return true;
    }
    private struct ScoredCandidate
    {
        public Vector2Int pos;
        public float score;
    }
    private float EvaluatePathComplexity(Vector2Int goalCell, Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        // Calcular una complejidad para el camino desde start hasta goal (más variedad)
        // El score depende del num de giros, curvas y distancia Manhattan
        int turns = 0;
        Vector2Int prevDir = Vector2Int.zero;
        Vector2Int cur = goalCell;

        // Recorrer de atras a lante
        while (cur != start)
        {
            Vector2Int parent = cameFrom[cur];
            Vector2Int dir = cur - parent;
            // No contar el primer paso y si hay un cambio de dir es un giro
            if (prevDir != Vector2Int.zero && dir != prevDir) turns++;
            prevDir = dir;
            cur = parent;
        }

        // Distancia Manhattan
        int manhattan = Mathf.Abs(goalCell.x - start.x) + Mathf.Abs(goalCell.y - start.y);

        // Formula inventada para el score
        // Los giros tienen mas peso que la distancia Manhattan
        return turns * 2f + manhattan * 0.25f;
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


    private void ComputeDistanceMap()
    {
        distanceMap = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                distanceMap[x, y] = -1; // -1 = inaccesible

        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(goal);
        distanceMap[goal.x, goal.y] = 0;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var d in dirs)
            {
                var nxt = cur + d;
                if (nxt.x < 0 || nxt.y < 0 || nxt.x >= width || nxt.y >= height) continue;
                if (grid[nxt.x, nxt.y] == 1) continue; // obstáculo
                if (distanceMap[nxt.x, nxt.y] != -1) continue; // ya visitado

                distanceMap[nxt.x, nxt.y] = distanceMap[cur.x, cur.y] + 1;
                q.Enqueue(nxt);
            }
        }
    }
    public int GetDistanceToGoal(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= width || pos.y >= height)
            return -1;
        return distanceMap[pos.x, pos.y];
    }
    public float GetProgress(Vector2Int pos)
    {
        int distStart = distanceMap[start.x, start.y];
        int distPlayer = GetDistanceToGoal(pos);

        if (distStart <= 0 || distPlayer < 0) return 0f;
        return 1f - (distPlayer / (float)distStart);
    }
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        // mismo cálculo que en BuildSceneFromGrid para origin
        Vector3 origin = transform.position - new Vector3((width - 1) * 0.5f * cellSize, 0f, (height - 1) * 0.5f * cellSize);

        // pasar de coordenada mundo a coordenada grid
        int x = Mathf.RoundToInt((worldPos.x - origin.x) / cellSize);
        int y = Mathf.RoundToInt((worldPos.z - origin.z) / cellSize); // OJO: z → y en tu grid

        // Clamp para no salirte
        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);

        return new Vector2Int(x, y);
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
    // Pasar a un array 1D  para enviar por red
    int[] FlattenGrid(int[,] grid, int width, int height)
    {
        int[] flat = new int[width * height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                flat[y * width + x] = grid[x, y]; // fila mayor
        return flat;
    }
    // Reconstruir grid 2D desde array 1D
    void LoadGrid(int[] flat, int width, int height)
    {
        grid = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = flat[y * width + x];
    }
    private void BuildSceneFromGrid()
    {
        // Crear un padre para agrupar los elementos del nivel
        GameObject root = new GameObject("GridLevel");
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
                    GameObject floor = Instantiate(floorPrefab, worldPos, floorRot, root.transform);
                    spawned.Add(floor);
                }

                int val = grid[x, y];
                GameObject toSpawn = null;
                Quaternion rot = Quaternion.identity;

                switch (val)
                {
                    case 1:
                        int idx = GetRandomObstacleIndex();
                        toSpawn = obstaclePrefabs[idx];
                        rot = toSpawn.transform.rotation;
                        break;
                    case 2:
                        toSpawn = startPrefab;
                        rot = markerRot; // rotar para que quede plano
                        break;
                    case 3:
                        toSpawn = goalPrefab;
                        //rot = markerRot; // rotar para que quede plano
                        break;
                }

                // Instanciar si hay prefab asignado
                if (toSpawn != null)
                {
                    GameObject go = Instantiate(toSpawn, worldPos, rot, root.transform);
                    spawned.Add(go);
                }
            }
        }
        if (wallPrefabs != null && wallPrefabs.Length >= 4)
        {
            // Rotaciones para las esquinas
            Quaternion rotInfIzq = Quaternion.Euler(0, 0, 0);
            Quaternion rotInfDer = Quaternion.Euler(0, -90, 0);
            Quaternion rotSupDer = Quaternion.Euler(0, 180, 0);
            Quaternion rotSupIzq = Quaternion.Euler(0, 90, 0);

            // Root distinta para los walls
            GameObject wallRoot = new GameObject("Walls");
            wallRoot.transform.SetParent(transform, false);

            // Esquinas 
            spawned.Add(Instantiate(wallPrefabs[0], origin + new Vector3(-1 * cellSize, 0f, -1 * cellSize), rotInfIzq, wallRoot.transform));
            spawned.Add(Instantiate(wallPrefabs[0], origin + new Vector3(width * cellSize, 0f, -1 * cellSize), rotInfDer, wallRoot.transform));
            spawned.Add(Instantiate(wallPrefabs[0], origin + new Vector3(width * cellSize, 0f, height * cellSize), rotSupDer, wallRoot.transform));
            spawned.Add(Instantiate(wallPrefabs[0], origin + new Vector3(-1 * cellSize, 0f, height * cellSize), rotSupIzq, wallRoot.transform));

            // Puerta
            spawned.Add(Instantiate(wallPrefabs[3], origin + new Vector3(0, 0f, -1 * cellSize), wallPrefabs[3].transform.rotation, wallRoot.transform));

            // Muros horizontales inferiores
            for (int x = 1; x < width; x++)
            {
                Vector3 posInf = origin + new Vector3(x * cellSize, 0f, -1 * cellSize);
                GameObject prefab = (x == width - 1) ? wallPrefabs[1] : (UnityEngine.Random.value < 0.7f ? wallPrefabs[1] : wallPrefabs[2]);
                spawned.Add(Instantiate(prefab, posInf, wallPrefabs[1].transform.rotation, wallRoot.transform));
            }
            // Muros horizontales superiores
            for (int x = 0; x < width; x++)
            {
                Vector3 posSup = origin + new Vector3(x * cellSize, 0f, height * cellSize);
                GameObject prefab = (x == 0 || x == width - 1) ? wallPrefabs[1] : (UnityEngine.Random.value < 0.7f ? wallPrefabs[1] : wallPrefabs[2]);
                spawned.Add(Instantiate(prefab, posSup, wallPrefabs[1].transform.rotation * Quaternion.Euler(0, 180, 0), wallRoot.transform));
            }

            // Muros verticales izquierdos 
            Quaternion verticalRot = wallPrefabs[1].transform.rotation * Quaternion.Euler(0f, 90f, 0f);
            for (int y = 0; y < height; y++)
            {
                Vector3 posIzq = origin + new Vector3(-1 * cellSize, 0f, y * cellSize);
                //GameObject prefab = (y == 0 || y == height - 1) ? wallPrefabs[1] : (Random.value < 0.8f ? wallPrefabs[1] : wallPrefabs[2]);
                GameObject prefab = wallPrefabs[1];
                spawned.Add(Instantiate(prefab, posIzq, verticalRot, wallRoot.transform));
            }
            // Muros verticales derechos 
            Quaternion verticalRotDer = wallPrefabs[1].transform.rotation * Quaternion.Euler(0f, 270f, 0f);
            for (int y = 0; y < height; y++)
            {
                Vector3 posDer = origin + new Vector3(width * cellSize, 0f, y * cellSize);
                //GameObject prefab = (y == 0 || y == height - 1) ? wallPrefabs[1] : (Random.value < 0.8f ? wallPrefabs[1] : wallPrefabs[2]);
                GameObject prefab = wallPrefabs[1];
                spawned.Add(Instantiate(prefab, posDer, verticalRotDer, wallRoot.transform));
            }
        }
    }
    void SendGridToClients()
    {
        int[] flatGrid = FlattenGrid(grid, width, height);
        ReceiveLevelClientRpc(flatGrid, width, height, start, goal);
    }
    [Rpc(SendTo.Everyone)]
    void ReceiveLevelClientRpc(int[] flatGrid, int width, int height, Vector2Int start, Vector2Int goal)
    {
        Debug.Log("Recibiendo nivel en cliente");

        this.width = width;
        this.height = height;
        LoadGrid(flatGrid, width, height);
        this.start = start;
        this.goal = goal;
        BuildSceneFromGrid();
    }
    private void ClearSpawned()
    {
        // Eliminar instancias previas
        foreach (var go in spawned)
        {
            if (go != null)
            {
                DestroyImmediate(go);
            }
        }
        spawned.Clear();

        var child = transform.Find("GridLevel");
        if (child != null)
        {
            DestroyImmediate(child.gameObject);
        }
    }
    private void GetFreeCells()
    {
        freeCells = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == 0)
                {
                    freeCells.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    public Vector3 GetRandomFreeCell()
    {
        Vector2Int randomCell = freeCells[UnityEngine.Random.Range(0, freeCells.Count)];

        Vector3 origin = transform.position - new Vector3(
            (width - 1) * 0.5f * cellSize,
            0f,
            (height - 1) * 0.5f * cellSize
        );

        float worldX = origin.x + randomCell.x * cellSize;
        float worldZ = origin.z + randomCell.y * cellSize;
        float worldY = origin.y + cellSize * 0.5f;

        return new Vector3(worldX, worldY, worldZ);
    }
    // Algoritmo de Fisher-Yates para barajar un array
    private void Shuffle(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int j = UnityEngine.Random.Range(i, array.Length);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
    private int GetRandomObstacleIndex()
    {
        float[] chances = { 0.45f, 0.45f, 0.1f };
        float r = UnityEngine.Random.value;
        float acc = 0f;
        for (int i = 0; i < chances.Length; i++)
        {
            acc += chances[i];
            if (r < acc)
                return i;
        }
        return chances.Length - 1; // fallback
    }
    public void GetSnapshot(out int w, out int h, out int[] flatGrid, out Vector2Int s, out Vector2Int g)
    {
        w = width;
        h = height;
        flatGrid = FlattenGrid(grid, width, height);
        s = start;
        g = goal;
    }
}
