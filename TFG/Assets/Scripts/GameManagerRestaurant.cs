using System;
using System.Linq;
using Unity.Netcode;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManagerRestaurant : NetworkBehaviour
{
    [Header("Evento")]
    [SerializeField] private ChooseGroup chooseGroup;
    [Header("Tiempo")]
    [SerializeField] private Countdown countdown;
    [Header("Cámara")]
    [SerializeField] private CameraSelector cameraSelector;
    [Header("Música")]
    [SerializeField] private AudioSource restaurantMusic;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer)
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
        countdown.CambiarVariable();
        
        
        foreach (ulong playerId in ChooseGroup.connectedPlayers)
        {
            NetworkObject player = NetworkManager.ConnectedClients[playerId].PlayerObject;
            SetCameraRPC(player.GetComponent<PlayerStats>().idGrupo.Value, playerId);
            PlayerInput playerInput = player.GetComponentInChildren<PlayerInput>();
            ActivatePlayerInputRPC(playerInput.GetComponent<NetworkObject>());
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ActivatePlayerInputRPC(NetworkObjectReference playerInputNetworkObjectReference)
    {
        playerInputNetworkObjectReference.TryGet(out NetworkObject playerInputNetworkObject);
        PlayerController playerController = playerInputNetworkObject.GetComponent<PlayerController>();
        playerController.enabled = true;
        ActivateRestaurantMusic();
    }
    private void ActivateRestaurantMusic()
    {
        restaurantMusic.Play();
    }
    [Rpc(SendTo.Everyone)]
    private void SetCameraRPC(int groupID, ulong id)
    {
        if (id != NetworkManager.Singleton.LocalClientId) return;
        cameraSelector.ActivateCamera(groupID);
    }
}
