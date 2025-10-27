using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenTutorialBotsManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TutorialGroupBehaviour groupBehaviour;
    [SerializeField] private LocalTurnBehaviour localTurnBehaviour;
    [SerializeField] private GridLevelGenerator gridLevelGenerator;

    [Header("Configuración")]
    public int botsCount = 2; 
    // Dar sensacion de pensar
    public float minThinkTime = 0.12f;
    public float maxThinkTime = 0.4f;

    private Coroutine botCoroutine;
    private bool isRunning;

    private readonly (Vector2Int dir, CommandType cmd)[] directions = new[]
    {
        (Vector2Int.up, CommandType.MoveUp),
        (Vector2Int.down, CommandType.MoveDown),
        (Vector2Int.left, CommandType.MoveLeft),
        (Vector2Int.right, CommandType.MoveRight)
    };

    public void RequestBotActions()
    {
        if(isRunning) return; // ya corriendo
        if (botCoroutine != null) return;
        botCoroutine = StartCoroutine(BotActionsCoroutine());
    }
    public void PauseBots()
    {
        if (botCoroutine != null)
        {
            StopCoroutine(botCoroutine);
            botCoroutine = null;
        }
        isRunning = false;
    }

    private IEnumerator BotActionsCoroutine()
    {
        isRunning = true;

        PlayerInputController player = FindFirstObjectByType<PlayerInputController>();
        
        Vector2Int pPos = player.CurrentGridPos;

        // Reservas de posiciones para evitar colisiones entre bots
        HashSet<Vector2Int> reserved = new HashSet<Vector2Int> { pPos };
        for (int i = 0; i < botsCount; i++)
        {
            yield return new WaitForSeconds(Random.Range(minThinkTime, maxThinkTime));

            CommandType chosen = DecideNextMove(pPos, reserved);

            // Meter nueva comanda
            groupBehaviour.AddCommand(chosen);

            // Avanzar 
            Vector2Int dest = GetDestinationForCommand(pPos, chosen);
            // No obstaculos
            if (dest != pPos && gridLevelGenerator.GetDistanceToGoal(dest) >= 0)
            {
                pPos = dest;
                reserved.Add(pPos);
            }

            yield return new WaitForSeconds(0.04f);
        }
        isRunning = false;
        botCoroutine = null;
        // Le toca al jugador
        localTurnBehaviour.NotifyBotsFinished();
    }

    // Selecciona el mejor movimiento que reduzca la distancia a la meta
    private CommandType DecideNextMove(Vector2Int origin, HashSet<Vector2Int> reserved)
    {
        int originDist = gridLevelGenerator.GetDistanceToGoal(origin);
        if (originDist <= 0) return CommandType.Wait; // ya esta en meta

        // Ver candidatos
        List<(Vector2Int pos, CommandType command, int dist)> improving = new List<(Vector2Int pos, CommandType cmd, int dist)>();
        foreach (var d in directions)
        {
            Vector2Int nxt = origin + d.dir;
            int dd = gridLevelGenerator.GetDistanceToGoal(nxt);
            if (dd < 0) continue;              // obstaculo
            if (dd >= originDist) continue;    // no mejora 
            if (reserved.Contains(nxt)) continue; // no pisar reservados

            // BFS para ver que se se puede llegar a meta desde ahi
            if (!LocalPathExists(origin, nxt, reserved)) continue;

            improving.Add((nxt, d.cmd, dd));
        }

        if (improving.Count == 0)
            return CommandType.Wait;

        // Elegir el que mas reduce distancia
        improving.Sort((a, b) => a.dist.CompareTo(b.dist));
        return improving[0].command;
    }

    private Vector2Int GetDestinationForCommand(Vector2Int origin, CommandType command)
    {
        switch (command)
        {
            case CommandType.MoveUp: return origin + Vector2Int.up;
            case CommandType.MoveDown: return origin + Vector2Int.down;
            case CommandType.MoveLeft: return origin + Vector2Int.left;
            case CommandType.MoveRight: return origin + Vector2Int.right;
            default: return origin;
        }
    }

    // Verifica que desde next sigue existiendo camino a la meta
    private bool LocalPathExists(Vector2Int from, Vector2Int next, HashSet<Vector2Int> blocked)
    {
        // No obstaculo
        if (gridLevelGenerator.GetDistanceToGoal(next) < 0) return false;

        // BFS desde 'next' hasta la meta 
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        q.Enqueue(next);
        visited.Add(next);

        Vector2Int[] dir = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            // En meta
            if (gridLevelGenerator.GetDistanceToGoal(cur) == 0) return true;

            foreach (var d in dir)
            {
                var nxt = cur + d;
                if (visited.Contains(nxt)) continue; // ya visitado
                if (blocked.Contains(nxt)) continue; // bloqueado por bots
                if (gridLevelGenerator.GetDistanceToGoal(nxt) < 0) continue; // obstaculo

                visited.Add(nxt);
                q.Enqueue(nxt);
            }
        }
        return false;
    }
}
