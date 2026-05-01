using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Gestiona y muestra la puntuacion.
/// Se suscribe automaticamente a todos los IScoreEvent de la escena.
/// </summary>
public class Score : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private int score = 0;
    private List<IScoreEvent> scoreEvents = new();

    private void Start()
    {
        // Buscar todos los IScoreEvent en la escena y suscribirse
        foreach (var obj in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (obj is IScoreEvent scoreEvent)
            {
                scoreEvent.OnScorePoint += AddScore;
                scoreEvents.Add(scoreEvent);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var scoreEvent in scoreEvents)
            scoreEvent.OnScorePoint -= AddScore;
        scoreEvents.Clear();
    }

    private void AddScore(int points)
    {
        score += points;
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    public void ResetScore()
    {
        score = 0;
        if (scoreText != null)
            scoreText.text = "0";
    }
}
