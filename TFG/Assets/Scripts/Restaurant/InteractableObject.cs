using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class InteractableObject : NetworkBehaviour
{
    //we assign all the renderers here through the inspector
    [SerializeField]
    Renderer renderers;
    [SerializeField]
    private Color color = new Color(68,68,68,255);

    //helper list to cache all the materials ofd this object
    private List<Material> materials;

    //Gets all the materials from each renderer
    private void Awake()
    {
        materials = new List<Material>();
        renderers = GetComponent<Renderer>();
        materials.AddRange(renderers.materials);
    }

    public void toggleHighlight(bool val)
    {
        if (val)
        {
            foreach (var material in materials)
            {
                //We need to enable the EMISSION
                material.EnableKeyword("_EMISSION");
                //before we can set the color
                material.SetColor("_EmissionColor", color);
            }
        }
        else
        {
            foreach (var material in materials)
            {
                //we can just disable the EMISSION
                //if we don't use emission color anywhere else
                material.DisableKeyword("_EMISSION");
            }
        }
    }

    public virtual void Interact(PlayerCarry player) 
    {
        
    }
}
