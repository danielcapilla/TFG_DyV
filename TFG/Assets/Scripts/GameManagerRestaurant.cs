using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManagerRestaurant : NetworkBehaviour
{
    [Header("Tiempo")]
    [SerializeField] private Countdown countdown;
    [Header("Camara")]
    [SerializeField] private CameraSelector cameraSelector;
    [Header("Musica")]
    [SerializeField] private AudioSource restaurantMusic;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            ChooseGroup.OnGameStartEvent -= StartGame;
            ChooseGroup.OnGameStartEvent += StartGame;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
            ChooseGroup.OnGameStartEvent -= StartGame;
    }

    private void StartGame()
    {
        // Copiar la lista ANTES del yield — ChooseGroup la limpia despues de disparar el evento
        List<ulong> players = new List<ulong>(ChooseGroup.connectedPlayers ?? new List<ulong>());
        StartCoroutine(StartGameDelayed(players));
    }

    private IEnumerator StartGameDelayed(List<ulong> players)
    {
        // Esperar a que PlayerSpawner haya spawneado los jugadores
        yield return new WaitForSeconds(0.5f);

        countdown.CambiarVariable();

        foreach (ulong playerId in players)
        {
            if (!NetworkManager.ConnectedClients.ContainsKey(playerId)) continue;
            NetworkObject playerStats = NetworkManager.ConnectedClients[playerId].PlayerObject;
            if (playerStats == null) continue;

            SetCameraRpc(playerStats.GetComponent<PlayerStats>().idGrupo.Value, playerId);

            // El jugador controlable es el hijo del PlayerObject spawneado por PlayerSpawner
            if (playerStats.transform.childCount > 0)
            {
                NetworkObject playerNetObj = playerStats.transform.GetChild(0)
                    .GetComponent<NetworkObject>();
                if (playerNetObj != null)
                    ActivatePlayerControllerRpc(playerNetObj);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ActivatePlayerControllerRpc(NetworkObjectReference playerRef)
    {
        if (!playerRef.TryGet(out NetworkObject playerNetObj)) return;
        PlayerController controller = playerNetObj.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;
        ActivateRestaurantMusic();
    }

    private void ActivateRestaurantMusic() => restaurantMusic.Play();

    [Rpc(SendTo.Everyone)]
    private void SetCameraRpc(int groupID, ulong id)
    {
        if (id != NetworkManager.Singleton.LocalClientId) return;
        cameraSelector.ActivateCamera(groupID);
    }
}
