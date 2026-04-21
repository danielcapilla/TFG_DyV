using UnityEngine;
using UnityEngine.EventSystems;

public static class UIInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Object.DontDestroyOnLoad(es); //stays across scenes
        }
    }
}

