using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateSpawner : InteractableObject
{
    [SerializeField] GameObject plate;
    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);
        if (!player.isCarrying)
        {
            GameObject instance = Instantiate(plate);
            player.carryObject(instance.GetComponent<PlateBehaviour>());
        }
    }
}
