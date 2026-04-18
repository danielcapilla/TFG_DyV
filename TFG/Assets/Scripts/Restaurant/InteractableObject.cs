using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class InteractableObject : NetworkBehaviour
{
    Renderer[] renderers;
    [SerializeField] private Color color = new Color(68, 68, 68, 255);
    private List<Material> materials;

    protected bool IsOffline => !NetworkManager.Singleton || !NetworkManager.Singleton.IsListening;

    private void Start()
    {
        materials = new List<Material>();
        renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            materials.AddRange(r.materials);
    }

    public void toggleHighlight(bool val)
    {
        if (materials == null) return;
        foreach (var mat in materials)
        {
            if (val) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", color); }
            else       mat.DisableKeyword("_EMISSION");
        }
    }

    /// <summary>
    /// Punto de entrada unico. En red llama a la logica de red; en offline llama a InteractOffline.
    /// </summary>
    public void Interact(PlayerCarry player)
    {
        if (IsOffline)
            InteractOffline(player);
        else
            InteractOnline(player);
    }

    /// <summary>Logica en red (RPCs). Sobreescribir en cada hijo.</summary>
    protected virtual void InteractOnline(PlayerCarry player) { }

    /// <summary>Logica offline sin red. Sobreescribir en cada hijo.</summary>
    protected virtual void InteractOffline(PlayerCarry player) { }
}
