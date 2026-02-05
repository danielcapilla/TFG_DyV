using UnityEngine;

public class Timer
{
    private float startTime;
    private bool isRunning;

    /// <summary>
    /// Inicia el cronómetro.
    /// </summary>
    public void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
    }

    /// <summary>
    /// Detiene el cronómetro.
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// Obtiene el tiempo transcurrido en segundos.
    /// </summary>
    /// <returns>Tiempo transcurrido en segundos.</returns>
    public float GetElapsedTime()
    {
        if (!isRunning)
            return 0f;

        return Time.time - startTime;
    }

    /// <summary>
    /// Reinicia el cronómetro a cero.
    /// </summary>
    public void ResetTimer()
    {
        startTime = 0f;
        isRunning = false;
    }

    /// <summary>
    /// Verifica si el cronómetro está en ejecución.
    /// </summary>
    /// <returns>True si está corriendo, false si no.</returns>
    public bool IsRunning()
    {
        return isRunning;
    }
}
