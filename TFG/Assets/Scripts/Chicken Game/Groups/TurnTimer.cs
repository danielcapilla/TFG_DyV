using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
public class TurnTimer : NetworkBehaviour
{
    [Header("Configuración")]
    public float duration = 10f;
    public float updateInterval = 0.2f; // Actualizar cada 200ms

    // Eventos (usando los action para hacerlos de otra manera)
    public event Action OnTimerStarted;
    public event Action OnTimerEnded;
    public event Action<float> OnTimeUpdated; // tiempo restante

    public NetworkVariable<float> timeRemaining = new NetworkVariable<float>(0f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isRunning = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Para controlar la corrutina de actualización
    private Coroutine updateCoroutine;
    private float lastUpdateTime;

    public override void OnNetworkSpawn()
    {
        timeRemaining.OnValueChanged += HandleTimeChanged;
        isRunning.OnValueChanged += HandleRunningStateChanged;

        // Inicializar con los valores actuales
        if (isRunning.Value)
        {
            OnTimerStarted?.Invoke();
        }
        OnTimeUpdated?.Invoke(timeRemaining.Value);
    }
    public float GetProgress() => 1f - (timeRemaining.Value / duration);
    private void HandleTimeChanged(float oldValue, float newValue)
    {
        OnTimeUpdated?.Invoke(newValue);

        // Verificar si el tiempo llego a cero
        if (newValue <= 0f && oldValue > 0f)
        {
            HandleTimerEnd();
        }
    }

    private void HandleRunningStateChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            OnTimerStarted?.Invoke();
            if (IsServer) StartUpdateCoroutine();
        }
        else
        {
            if (IsServer && updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
        }
    }

    private void HandleTimerEnd()
    {
        isRunning.Value = false;
        OnTimerEnded?.Invoke();
    }

    // Corrutina que se ejecuta solo en el servidor para actualizar el tiempo
    private IEnumerator UpdateTimerCoroutine()
    {
        while (isRunning.Value && timeRemaining.Value > 0f)
        {
            // Esperar el intervalo deseado
            yield return new WaitForSeconds(updateInterval);

            // Actualizar el tiempo restante
            timeRemaining.Value -= updateInterval;

            // Asegurarse de que no sea negativo
            if (timeRemaining.Value < 0f)
            {
                timeRemaining.Value = 0f;
            }
        }

        // Si salimos del bucle, el tiempo termino
        if (timeRemaining.Value <= 0f)
        {
            HandleTimerEnd();
        }

        updateCoroutine = null;
    }

    private void StartUpdateCoroutine()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
        updateCoroutine = StartCoroutine(UpdateTimerCoroutine());
    }

    public void StartTimer()
    {
        if (IsServer)
        {
            timeRemaining.Value = duration;
            isRunning.Value = true;
        }
        else
        {
            StartTimerServerRpc();
        }
    }

    public void StopTimer()
    {
        if (IsServer)
        {
            isRunning.Value = false;
        }
        else
        {
            StopTimerServerRpc();
        }
    }

    public void ResetTimer()
    {
        if (IsServer)
        {
            timeRemaining.Value = duration;
        }
        else
        {
            ResetTimerServerRpc();
        }
    }

    // RPCs para control desde clientes
    [Rpc(SendTo.Server)]
    private void StartTimerServerRpc()
    {
        StartTimer();
    }

    [Rpc(SendTo.Server)]
    private void StopTimerServerRpc()
    {
        StopTimer();
    }

    [Rpc(SendTo.Server)]
    private void ResetTimerServerRpc()
    {
        ResetTimer();
    }

    

    public override void OnNetworkDespawn()
    {
        // Limpiar suscripciones
        timeRemaining.OnValueChanged -= HandleTimeChanged;
        isRunning.OnValueChanged -= HandleRunningStateChanged;

        // Detener corrutinas
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
    }
}
