using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class RecipeRandomizer : NetworkBehaviour
{
    public MenuScriptableObject currentMenu;
    [SerializeField] Dictionary<IngredientsScriptableObject, int> pairedIngredients = new Dictionary<IngredientsScriptableObject, int>();
    [SerializeField] public List<List<IngredientsScriptableObject>> recipes = new List<List<IngredientsScriptableObject>>();
    [SerializeField] List<IngredientsScriptableObject> extraIngredients = new List<IngredientsScriptableObject>();
    public List<List<IngredientsScriptableObject>> currentOrders = new List<List<IngredientsScriptableObject>>();

    private NetworkVariable<int> randomSeed = new();

    CommandSpawner commandSpawner;

    private Countdown countdown;

    public override void OnNetworkSpawn()
    {
        commandSpawner = GetComponent<CommandSpawner>();
        countdown = GetComponent<Countdown>();
        countdown.OnRegresiveTimerFinished += SpawnRestaurantUI;
        randomSeed.OnValueChanged += SetRandomSeed;
        
        if (IsServer || IsHost)
        {
            randomSeed.Value = Random.Range(int.MinValue, int.MaxValue);
        }
        else
        {
            Random.InitState(randomSeed.Value);
        }
    }

    private void SpawnRestaurantUI(object sender, System.EventArgs e)
    {
        RandomizeIngredients();
        GenerateRandomRecipes();
        commandSpawner.SpawnRecipes(recipes, pairedIngredients);
        GenerateRandomOrder();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        randomSeed.OnValueChanged -= SetRandomSeed;
        countdown.OnRegresiveTimerFinished -= SpawnRestaurantUI;
    }
    private void SetRandomSeed(int current, int newValue)
    {
        randomSeed.Value = newValue;
        Random.InitState(randomSeed.Value);
    }
    // Start is called before the first frame update
    void Start()
    {
        //Si es host o server

        
        
        //TODO shuffle recipe appearing order

        //Si es cliente recibe el randomizado hecho por el host o server

        //Muestra las comandas

        //Si es host o server
        //Solicita pedido

        //Si es cliente recibe pedido del host o server

        //Muestra pedido
    }

    public void RandomizeIngredients() 
    {
        //Asigna imagenes a los ingredientes sin que se repitan

        //Para la carne en el caso de la hamburguesa
        foreach (IngredientsScriptableObject ingredient in currentMenu.importantIngredients)
        {
            int rand = Random.Range(0, commandSpawner.Codes.Count);
            while (pairedIngredients.ContainsValue(rand))
            {
                rand = Random.Range(0, commandSpawner.Codes.Count);
            }
            pairedIngredients[ingredient] = rand;
        }
        //Para el queso, lechuga... en el caso de la hamburguesa
        foreach (IngredientsScriptableObject ingredient in currentMenu.extraIngredients) 
        {
            int rand = Random.Range(0,commandSpawner.Codes.Count);
            while (pairedIngredients.ContainsValue(rand)) 
            {
                rand = Random.Range(0, commandSpawner.Codes.Count);
            }
            pairedIngredients[ingredient] = rand;
        }
    }

    public void GenerateRandomOrder() 
    {
        for (int j = 0; j < 3; j++)
        {

            List<IngredientsScriptableObject> currentOrder = new List<IngredientsScriptableObject>();
            //Añadir pan
            foreach (IngredientsScriptableObject coreIngredient in currentMenu.coreIngredients)
            {
                currentOrder.Add(coreIngredient);
            }
            //Añadir 1 carne
            int importantIngredientIDX = Random.Range(0, currentMenu.importantIngredients.Count);
            currentOrder.Add(currentMenu.importantIngredients[importantIngredientIDX]);
            //Añadir extras (considerar posibilidad de añadir la misma carne como extra)
            int burguerSize = Random.Range(2, 5);
            for (int i = 0; i < burguerSize; i++)
            {
                int ingredientIDX = Random.Range(0, currentMenu.extraIngredients.Count + 1);
                //We added a fake extra ingredient as meat duplicate
                if (ingredientIDX == currentMenu.extraIngredients.Count)
                {
                    currentOrder.Add(currentMenu.importantIngredients[importantIngredientIDX]);
                    continue;
                }
                currentOrder.Add(currentMenu.extraIngredients[ingredientIDX]);
            }

            //Añadir pan
            foreach (IngredientsScriptableObject coreIngredient in currentMenu.coreIngredients)
            {
                if (coreIngredient.MinimumQuantity > 1)
                {
                    currentOrder.Add(coreIngredient);
                }
            }

            currentOrders.Add(currentOrder);
        }
        commandSpawner.SpawnOrder(pairedIngredients, currentOrders[0]);
    }

    public void GenerateRandomRecipes() 
    {
        //HashSet<List<IngredientsScriptableObject>> recipes = new HashSet<List<IngredientsScriptableObject>>();
        extraIngredients.AddRange(currentMenu.extraIngredients);
        List<IngredientsScriptableObject> dupedIngredients = new List<IngredientsScriptableObject>();
        //Add random extra ingredients duplicates
        int randIng1 = Random.Range(0,currentMenu.extraIngredients.Count);
        int randIng2 = Random.Range(0, currentMenu.extraIngredients.Count);
        int randIng3 = Random.Range(0, currentMenu.extraIngredients.Count);
        //Avoid same ingredients
        while (randIng1 == randIng2) 
        {
            randIng2 = Random.Range(0, currentMenu.extraIngredients.Count);
        }
        while (randIng3 == randIng2 || randIng3 == randIng1)
        {
            randIng3 = Random.Range(0, currentMenu.extraIngredients.Count);
        }
        //Add 1 extra of one type and 2 of the other
        dupedIngredients.Add(currentMenu.extraIngredients[randIng1]);
        dupedIngredients.Add(currentMenu.extraIngredients[randIng2]);
        dupedIngredients.Add(currentMenu.extraIngredients[randIng3]);
        dupedIngredients.Add(currentMenu.extraIngredients[randIng1]);
        dupedIngredients.Add(currentMenu.extraIngredients[randIng2]);
        dupedIngredients.Add(currentMenu.extraIngredients[randIng3]);

        extraIngredients.Remove(currentMenu.extraIngredients[randIng1]);
        extraIngredients.Remove(currentMenu.extraIngredients[randIng2]);
        extraIngredients.Remove(currentMenu.extraIngredients[randIng3]);

        for (int i = 0; i < currentMenu.NumberOfExampleRecipes; i++) 
        {
            
            List<IngredientsScriptableObject> recipe = new List<IngredientsScriptableObject>();
            //Add core ingredients as base
            foreach (IngredientsScriptableObject coreIngredient in currentMenu.coreIngredients) 
            {
                recipe.Add(coreIngredient);
            }

            //Add important ingredient if there are various options (ex meat in burguer) distribute in round robin
            recipe.Add(currentMenu.importantIngredients[i % currentMenu.importantIngredients.Count]);

            //Add extra ingredients
            int extraIngredientsToAdd = 1;
            if (i == 0) 
            {
                extraIngredientsToAdd = 0;
                recipe.Add(dupedIngredients[0]);
                dupedIngredients.RemoveAt(0);
            }
            else if (i == 1) 
            {
                extraIngredientsToAdd = 0;
                recipe.Add(dupedIngredients[2]);
                dupedIngredients.RemoveAt(2);
                recipe.Add(dupedIngredients[1]);
                dupedIngredients.RemoveAt(1);
            }
            else if (dupedIngredients.Count > 0) 
            {
                recipe.Add(dupedIngredients[0]);
                dupedIngredients.RemoveAt(0);
            }


            for(int j = 0; j < extraIngredientsToAdd; j++) 
            {
                int rand = Random.Range(0, extraIngredients.Count);
                recipe.Add(extraIngredients[rand]);
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
            RandomizeRecipes();
        }
    }

    private void RandomizeRecipes()
    {
        recipes.Sort((a, b) => 1 - 2 * Random.Range(0, recipes.Count));
    }

    public void NextOrder(int order) 
    {
        commandSpawner.SpawnOrder(pairedIngredients, currentOrders[order]);
    }
}
