using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManagerChicken : NetworkBehaviour
{
    [Header("Evento")]
    [SerializeField] private ChooseGroup chooseGroup;
    //[Header("Tiempo")]
    //[SerializeField] private Countdown countdown;
    //[Header("Cámara")]
    //[SerializeField] private CameraSelector cameraSelector;
    //[Header("Música")]
    //[SerializeField] private AudioSource restaurantMusic;
    // Eventos
    public delegate void PlayerSpawned(NetworkObjectReference playerNOR, int idGroup);
    public event PlayerSpawned OnPlayerSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            chooseGroup.OnGameStartEvent += StartGame;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            chooseGroup.OnGameStartEvent -= StartGame;
        }
    }
    private void StartGame()
    {
        foreach (ulong playerId in ChooseGroup.connectedPlayers)
        {
            NetworkObject player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
            OnPlayerSpawned?.Invoke(player, player.GetComponent<PlayerStats>().idGrupo.Value);
        }
    }
}
