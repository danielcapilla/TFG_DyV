using UnityEngine;

public class GoalPulse : MonoBehaviour
{
    [Header("Variables")]
    public Renderer targetRenderer;
    public Color emissionColor = Color.yellow;
    public float pulseSpeed = 2f;   
    public float minIntensity = 0.5f;
    public float maxIntensity = 3f;

    private Material mat;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        mat = targetRenderer.material;
    }

    void Update()
    {
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        Color finalColor = emissionColor * intensity;
        mat.SetColor("_EmissionColor", finalColor);
        DynamicGI.SetEmissive(targetRenderer, finalColor);
    }
}
