using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerPipes : GameManagerBase
{
    [Header("Pipe Game - Referencias")]
    [SerializeField] private AudioSource pipesMusic;

    protected override void OnGameStarted()
    {
        //pipesMusic?.Play();
    }

    protected override void OnTimerFinished()
    {
        // Por implementar: guardar resultado y cambiar escena
        NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
    }
}
