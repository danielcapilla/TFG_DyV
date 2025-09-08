using UnityEngine;
using Unity.Netcode;
public class TurnTimer : NetworkBehaviour
{
    [Header("Tiempo de turno")]
    [SerializeField] private float turnDuration = 10f; 
    
    public NetworkVariable<float> timeRemaining = new NetworkVariable<float>(10f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isTimerRunning = new NetworkVariable<bool>(
        false,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Para reducir trafico de red, actualizamos cada 0.5 segundos
    private float lastUpdateTime;
    private const float UPDATE_FREQUENCY = 0.5f;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Solo el servidor controla el tiempo
        if (IsServer)
        {
            //ResetTimer();
        }

        // Todos los clientes se suscriben a los cambios
        timeRemaining.OnValueChanged += OnTimeChanged;
        isTimerRunning.OnValueChanged += OnTimerStateChanged;
    }
    private void OnTimeChanged(float oldValue, float newValue)
    {
        //UpdateTimerUI(newValue); // Solo UI
    }  
    private void OnTimerStateChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            Debug.Log("Timer iniciado");
        }
        else
        {
            Debug.Log("Timer detenido");
        }
    }
    void Update()
    {
        // Solo el servidor actualiza el tiempo y si esta corriendo
        if (!IsServer || !isTimerRunning.Value) return;


        timeRemaining.Value -= Time.deltaTime;

        // Actualizar clientes cada cierto tiempo
        if (Time.time - lastUpdateTime > UPDATE_FREQUENCY)
        {
            lastUpdateTime = Time.time;
            UpdateTimerClientRpc(timeRemaining.Value);
        }
        if (timeRemaining.Value <= 0f)
        {
            //TimeExpired();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartTimerServerRpc()
    {
        //ResetTimer();
        isTimerRunning.Value = true;
        StartTimerClientRpc();
    }

    [ClientRpc]
    private void StartTimerClientRpc()
    {
        // Efectos visuales/sonoros de inicio de timer
        Debug.Log("Timer iniciado en cliente");
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopTimerServerRpc()
    {
        isTimerRunning.Value = false;
        StopTimerClientRpc();
    }

    [ClientRpc]
    private void StopTimerClientRpc()
    {
        // Efectos visuales/sonoros de fin de timer
        Debug.Log("Timer detenido en cliente");
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(float currentTime)
    {
        // Sincronización periódica para mantener precisión
        timeRemaining.Value = currentTime;
    }
}
