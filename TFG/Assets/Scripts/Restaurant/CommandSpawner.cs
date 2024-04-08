using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommandSpawner : MonoBehaviour
{
    HashSet<List<IngredientsScriptableObject>> recipes = new HashSet<List<IngredientsScriptableObject>>();
    RecipeRandomizer randomizer;
    [SerializeField]
    private TextMeshProUGUI textMeshProUGUI;
    private void Awake()
    {
        randomizer = this.GetComponent<RecipeRandomizer>();
    }
    private void Start()
    {
        randomizer.GenerateRandomRecipes();
        //recipes = randomizer.GenerateRandomRecipes();
        
        foreach (List<IngredientsScriptableObject> recipe in RecipeRandomizer.recipes) 
        {
            textMeshProUGUI.text += "[ ";
            foreach (IngredientsScriptableObject ingredient in recipe)
            {
                textMeshProUGUI.text += ingredient.name.ToString() + " ";
            }
            textMeshProUGUI.text += "] ";
        }
       
    }
}
