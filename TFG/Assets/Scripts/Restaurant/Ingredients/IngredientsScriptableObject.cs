using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Ingredient",menuName ="Ingredients/Ingredient")]
public class IngredientsScriptableObject : ScriptableObject
{
    public int MinimumQuantity;
    public string IngredientName;
    public GameObject Model;
    public Sprite Sprite;
    public Sprite AlternativeSprite;
    public IngredientRarity Rarity;
}
public enum IngredientRarity
{
    core, important, extra
}