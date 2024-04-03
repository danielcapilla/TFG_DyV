using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateBehaviour : NetworkBehaviour, ICarryObject
{
    List<IngredientBehaviour> Ingredients = new List<IngredientBehaviour>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddIngredient(IngredientBehaviour ingredient) 
    {
        //TODO add ingredient model to plate model
        ingredient.transform.parent = transform;
        if (Ingredients.Count > 0)
        {
            ingredient.transform.localPosition = new Vector3(0, Ingredients[Ingredients.Count - 1].transform.localPosition.y + Ingredients[Ingredients.Count-1].transform.localScale.y*2, 0);
        }
        else
        {
            ingredient.transform.localPosition = new Vector3(0, ingredient.transform.localScale.y*2, 0);
        }
        Ingredients.Add(ingredient);
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
