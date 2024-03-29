using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Menu", menuName = "Ingredients/Menu")]
public class MenuScriptableObject : ScriptableObject
{
    public List<IngredientsScriptableObject> coreIngredients;
    public List<IngredientsScriptableObject> otherIngredients;
}
