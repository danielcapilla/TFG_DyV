using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : InteractableObject
{
    bool isOccupied = false;
    [SerializeField] Transform placePosition;
    GameObject holdingObject;

    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);

        if (!isOccupied && player.isCarrying)
        {
            holdingObject = player.dropObject();
            holdingObject.transform.parent = placePosition;
            holdingObject.transform.localPosition = Vector3.zero;
            isOccupied = true;
        }
        else if (isOccupied && !player.isCarrying) 
        {
            holdingObject.transform.parent = null;
            player.carryObject(holdingObject);
            isOccupied = false;
        }
    }
}
