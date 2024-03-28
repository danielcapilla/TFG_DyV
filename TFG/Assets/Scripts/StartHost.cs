using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using UnityEngine;

public class StartHost : MonoBehaviour
{
    public void Starthost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void Startclient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
