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
        pipesMusic?.Play();
        string difficulty = config != null ? config.difficulty : "Normal";
        recorder?.StartRecording(PlayerData.ClassCode, PlayerData.ClassCode, difficulty);
    }

    protected override void OnTimerFinished()
    {
        // Guardar partida y cambiar escena solo cuando el guardado termina
        if (recorder != null)
            recorder.SaveMatch(PlayerData.ClassCode, PlayerData.ClassCode, OnSaveComplete);
        else
            ChangeScene();
    }

    private void OnSaveComplete(int result)
    {
        ChangeScene();
    }

    private void ChangeScene()
    {
        if (IsOffline)
            SceneManager.LoadScene("Podium", LoadSceneMode.Single);
        else
            NetworkManager.Singleton.SceneManager.LoadScene("Podium", LoadSceneMode.Single);
    }
}
