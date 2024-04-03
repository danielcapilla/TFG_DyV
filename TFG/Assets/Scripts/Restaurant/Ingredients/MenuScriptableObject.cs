using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Menu", menuName = "Ingredients/Menu")]
public class MenuScriptableObject : ScriptableObject
{
    public List<IngredientsScriptableObject> coreIngredients;
    public List<IngredientsScriptableObject> importantIngredients;
    public List<IngredientsScriptableObject> extraIngredients;

    public int NumberOfExampleRecipes;
    public int MinIngredientsPerExample;
    public int MaxIngredientsPerExample;
}
