using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientBox : InteractableObject
{
    public IngredientsScriptableObject ingredient;

    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);

        if (!player.isCarrying)
        {
            Debug.Log("Spawneando: " + ingredient.IngredientName);
            GameObject instance = Instantiate(ingredient.Model);
            //TODO Add ingredient behaviour script
            player.carryObject(instance);
        }
    }

    //Missing box display its content and highlight when player can interact
}
