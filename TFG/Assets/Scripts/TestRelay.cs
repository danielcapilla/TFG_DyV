using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestRelay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI code;
    [SerializeField]
    private TextMeshProUGUI showCode;
    //Función asíncrona
    private async void Start()
    {
        //Inicializar los servicios de Unity
        //Es un a función asíncrona por lo tanto el método tiene que ser asinc
        //Evita que se congele para los demás usuarios
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        //Se inicia la sesión de manera anónima
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
    }
    [ClientRpc]

    private void ShowCodeClientRPC(string code)
    {
        showCode.text = code;
    }
    public async void CreateRelay()
    {
        try 
        {
            //El argumento es el número máximo de jugadores sin contar el host
            //Creamos una "conexión" con un código
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            //Código para iniciar la partida
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            //RelayServerData relayServerData = new RelayServerData(allocation,"dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );
            NetworkManager.Singleton.StartHost();
            ShowCodeClientRPC(joinCode);
            NetworkManager.Singleton.SceneManager.LoadScene("MinijuegoRestaurante", LoadSceneMode.Single);
            //this.gameObject.SetActive(false);
            Debug.Log(joinCode);
        } catch (RelayServiceException e)
        {
            Debug.Log("Error: " + e);
        }
        
    }
    public async void JoinRelay(string joinCode)
    {
        //Asignamos el texto que se ha introducido
        joinCode = code.text;
        joinCode = joinCode.ToUpper();
        try
        {
            //Problema que surge con el textMeshPro
            joinCode = joinCode.Substring(0, 6);
            Debug.Log("Joining Relay with " + joinCode);
            //Nos unimos mediante el código
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            //Manejamos el Relay a través de nuestro Unity Transport
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );
            NetworkManager.Singleton.StartClient();
        } catch (RelayServiceException e) 
        {
            Debug.Log("Error: " + e);
        }
    }

}
