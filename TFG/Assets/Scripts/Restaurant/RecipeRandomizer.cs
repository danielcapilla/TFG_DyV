using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeRandomizer : MonoBehaviour
{
    public MenuScriptableObject currentMenu;
    [SerializeField] List<Image> Codes;
    public Dictionary<int, IngredientsScriptableObject> pairedIngredients = new Dictionary<int, IngredientsScriptableObject>();

    // Start is called before the first frame update
    void Start()
    {
        //Si es host o server

        RandomizeIngredients();

        //Si es cliente recibe el randomizado hecho por el host o server
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RandomizeIngredients() 
    {
        foreach (IngredientsScriptableObject ingredient in currentMenu.importantIngredients)
        {
            int rand = Random.Range(0, Codes.Count);
            while (pairedIngredients.ContainsKey(rand))
            {
                rand = Random.Range(0, Codes.Count);
            }
            pairedIngredients[rand] = ingredient;
        }

        foreach (IngredientsScriptableObject ingredient in currentMenu.extraIngredients) 
        {
            int rand = Random.Range(0,Codes.Count);
            while (pairedIngredients.ContainsKey(rand)) 
            {
                rand = Random.Range(0, Codes.Count);
            }
            pairedIngredients[rand] = ingredient;
        }
    }

    public void GenerateRandomRecipe() 
    {
        //A�adir pan
        //A�adir 1 carne
        //A�adir extras (considerar posibilidad de a�adir la misma carne como extra)
        //A�adir pan
    }

    public void DisplayRecipes() 
    {
        HashSet<int> utilizados = new HashSet<int>();
        for (int i = 0; i < currentMenu.NumberOfExampleRecipes; i++) 
        {
            
        }
    }
}
