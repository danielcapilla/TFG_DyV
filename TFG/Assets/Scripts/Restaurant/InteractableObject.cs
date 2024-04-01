using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class InteractableObject : NetworkBehaviour
{
    public virtual void Interact(PlayerCarry player) 
    {
        
    }
}
