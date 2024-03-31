using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trashcan : InteractableObject
{
    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);

        CarryObject destroyObject = player.dropObject();
        if (destroyObject)
        {
            Destroy(destroyObject.gameObject);
        }
    }
}
