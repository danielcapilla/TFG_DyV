using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientBox : InteractableObject
{
    public IngredientsScriptableObject ingredient;

    private void Start()
    {
        GameObject instance = Instantiate(ingredient.Model,transform);
        instance.transform.localPosition = new Vector3(0,0.5f,0);
    }

    public override void Interact(PlayerCarry player)
    {
        base.Interact(player);

        if (!player.isCarrying)
        {
            Debug.Log("Spawneando: " + ingredient.IngredientName);
            GameObject instance = Instantiate(ingredient.Model);
            IngredientBehaviour carryObject = instance.AddComponent<IngredientBehaviour>();
            carryObject.ingredient = ingredient;
            player.carryObject(carryObject);
        }
    }

    //Missing box display its content and highlight when player can interact
}
