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
            else mat.DisableKeyword("_EMISSION");
        }
    }

    /// <summary>
    /// Punto de entrada. Recibe el GameObject del jugador.
    /// Cada implementacion hace GetComponent de lo que necesite,
    /// sin depender de una interfaz concreta.
    /// </summary>
    /// <summary>Indica si este objeto puede interactuarse ahora. Sobrescribir para logica custom.</summary>
    public virtual bool CanInteract(GameObject player) => true;

    public void Interact(GameObject player)
    {
        if (IsOffline) InteractOffline(player);
        else InteractOnline(player);
    }

    protected virtual void InteractOnline(GameObject player) { }
    protected virtual void InteractOffline(GameObject player) { }
}
