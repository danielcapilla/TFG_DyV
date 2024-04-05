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

    public void GenerateRandomOrder() 
    {
        //Añadir pan
        //Añadir 1 carne
        //Añadir extras (considerar posibilidad de añadir la misma carne como extra)
        //Añadir pan
    }

    public void GenerateRandomRecipes() 
    {
        HashSet<List<IngredientsScriptableObject>> recipes = new HashSet<List<IngredientsScriptableObject>>();
        List<IngredientsScriptableObject> extraIngredients = new List<IngredientsScriptableObject>();
        extraIngredients.AddRange(currentMenu.extraIngredients);
        //Add random extra ingredients duplicates

        for (int i = 0; i < currentMenu.NumberOfExampleRecipes; i++) 
        {
            int numberOfExtraIngredients = 0;
            List<IngredientsScriptableObject> recipe = new List<IngredientsScriptableObject>();
            //Add core ingredients as base
            foreach (IngredientsScriptableObject coreIngredient in currentMenu.coreIngredients) 
            {
                recipe.Add(coreIngredient);
            }

            //Add important ingredient if there are various options (ex meat in burguer) distribute in round robin
            recipe.Add(currentMenu.importantIngredients[i % currentMenu.importantIngredients.Count]);

            if (i == 0)
            {
                numberOfExtraIngredients = 1;
                //maybe store one dupe ingredient and manually add it instead
            }
            else 
            {
                numberOfExtraIngredients = 2;
            }

            


            //Add extra ingredients
            for (int j = 0; j < numberOfExtraIngredients; j++) 
            {
                //Choose random ingredient
                int rand = Random.Range(0,extraIngredients.Count);

                //If recipe already contains that ingredient choose another
                while (recipe.Contains(extraIngredients[rand])) 
                {
                    rand = Random.Range(0, extraIngredients.Count);
                    //TODO it can happen that dupe ingredients are left for last leading to infinite loop
                }

                //Add ingredient to recipe
                recipe.Add(extraIngredients[rand]);
                //Remove used ingredient
                extraIngredients.RemoveAt(rand);
            }

            //Add core ingredients again if needed
            foreach (IngredientsScriptableObject coreIngredient in currentMenu.coreIngredients)
            {
                if (coreIngredient.MinimumQuantity > 1) 
                {
                    recipe.Add(coreIngredient);
                }
            }

            recipes.Add(recipe);
        }
    }
}
