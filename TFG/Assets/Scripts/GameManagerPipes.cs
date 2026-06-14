using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerPipes : GameManagerBase
{
    [Header("Pipe Game - Referencias")]
    [SerializeField] private AudioSource pipesMusic;
    [SerializeField] private PipeMatchRecorder recorder;
    [SerializeField] private PipeGameConfigSO config;

    private bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    protected override void OnGameStarted()
    {
        //pipesMusic?.Play();
        string difficulty = config != null ? config.difficulty : "Normal";
        recorder?.StartRecording(PlayerData.ClassCode, PlayerData.ClassCode, difficulty);
        if (recorder != null)
        {
            recorder.OnMaxRoundsReached -= OnMaxRoundsHandler;
            recorder.OnMaxRoundsReached += OnMaxRoundsHandler;
            Debug.Log("[GameManagerPipes] Suscrito a OnMaxRoundsReached");
        }
        else
            Debug.LogWarning("[GameManagerPipes] recorder es null!");
    }

    private void OnMaxRoundsHandler()
    {
        Debug.Log("[GameManagerPipes] OnMaxRoundsReached recibido -> OnTimerFinished");
        OnTimerFinished();
    }

    protected override void OnTimerFinished()
    {
        Debug.Log("[GameManagerPipes] OnTimerFinished");
        if (recorder != null)
            recorder.SaveMatch(PlayerData.ClassCode, PlayerData.ClassCode, OnSaveComplete);
        else
            ChangeScene();
    }

    private void OnDestroy()
    {
        if (recorder != null)
            recorder.OnMaxRoundsReached -= OnMaxRoundsHandler;
    }

    private void OnSaveComplete(int result)
    {
        Debug.Log("[GameManagerPipes] OnSaveComplete result=" + result);
        ChangeScene();
    }

    private void ChangeScene()
    {
        Debug.Log("[GameManagerPipes] ChangeScene");
        if (IsOffline)
            SceneManager.LoadScene("Podium", LoadSceneMode.Single);
        else
            NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
    }
}
