using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalBehaviour : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Goal!");
            ChangeSceneRPC();
        }
    }
    [Rpc(SendTo.Server)]
    private void ChangeSceneRPC()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
    }
}
