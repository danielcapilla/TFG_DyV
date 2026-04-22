using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerUpManager : NetworkBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private GameObject[] powerUpPrefabs; 
    [SerializeField] private float spawnInterval = 10f; 
    [SerializeField] private int maxActivePowerUps = 3; 
    [Header("Referencias")]
    [SerializeField] private GridLevelGenerator gridGenerator;
    [SerializeField] private ChooseGroup chooseGroup;
    [SerializeField] private AudioSource popAppear;

    public static PowerUpManager Instance { get; private set; }

    private List<NetworkObject> activePowerUps = new List<NetworkObject>();
    private Coroutine spawnCoroutine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Solo el servidor maneja el spawn de powerUps
        if (!IsServer) return;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ChooseGroup.OnGameStartEvent += OnLevelGenerated;

    }

    private void OnLevelGenerated()
    {
        ClearAllPowerUps();
        StartSpawning();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsServer) return;
        ChooseGroup.OnGameStartEvent -= OnLevelGenerated;
        StopSpawning();
    }

    public void StartSpawning()
    {
        // Spawnear cada x tiempo
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnPowerUpsCoroutine());
    }

    public void StopSpawning()
    {
        // Parar el spawn
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        ClearAllPowerUps();
    }

    private void ClearAllPowerUps()
    {
        foreach (var powerUp in activePowerUps)
        {
            if (powerUp != null)
            {
                powerUp.Despawn();
                Destroy(powerUp.gameObject);
            }
        }

        activePowerUps.Clear();
    }

    private IEnumerator SpawnPowerUpsCoroutine()
    {
        // Esperar un poco antes del primer spawn
        yield return new WaitForSeconds(spawnInterval);

        while (true)
        {
            // Limpiar la lista de power-ups destruidos
            activePowerUps.RemoveAll(p => p == null);

            // Spawnear si es menor al limite
            if (activePowerUps.Count < maxActivePowerUps)
            {
                SpawnRandomPowerUp();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnRandomPowerUp()
    {
        if (powerUpPrefabs.Length == 0) return;

        GameObject selectedPrefab = powerUpPrefabs[UnityEngine.Random.Range(0, powerUpPrefabs.Length)];
        Vector3 spawnPosition = gridGenerator.GetRandomFreeCell();
        spawnPosition.y = 0.2f; // Altura

        GameObject powerUpInstance = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = powerUpInstance.GetComponent<NetworkObject>();

        networkObject.Spawn();
        //PlayPowerUpSoundRPC(); // Se se lo pongo en local me ahorro una rpc
        activePowerUps.Add(networkObject);
    }
    [Rpc(SendTo.Everyone)]
    public void PlayPowerUpSoundRPC()
    {
        popAppear.Play();
    }
}
public static class PowerUpEvents
{
    public static Action<float> OnInvertControls;
    public static void InvokeInvertControls(float duration)
    {
        OnInvertControls?.Invoke(duration);
    }
    public static Action<float> OnPlayerSpeedUp;
    public static void InvokePlayerSpeedUp(float duration)
    {
        OnPlayerSpeedUp?.Invoke(duration);
    }
}

