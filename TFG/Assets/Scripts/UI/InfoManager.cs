using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoManager : MonoBehaviour
{
    [SerializeField]
    private GameObject hamburguesasIzq;
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    public List<Sprite> Codes;
    [SerializeField]
    private FiltersBehaviour filtersBehaviour;
    [SerializeField]
    private IngredientsScriptableObject[] currentMenu;

    private void OnEnable()
    {
        SpawnRecipes(filtersBehaviour.match.HamburguesasEjemplo);
    }
    public void SpawnRecipes(List<List<BurguerJSONCreator.Ingredientes>> recipes)
    {

        for (int i = 0; i < recipes.Count; i++)
        {
            GameObject instance = Instantiate(panel, hamburguesasIzq.transform);
            //instance.transform.SetParent(this.transform.GetChild(0), false);
            RectTransform instanceRectTransform = instance.GetComponent<RectTransform>();
            //Para mover cosas en canvas usar anchoredPosition!!!!
            instanceRectTransform.anchoredPosition = new Vector3(instanceRectTransform.sizeDelta.x * i, 0f, 0f);

            //instance.GetComponentInChildren<TextMeshProUGUI>().text += "[ ";
            int j = 0;
            foreach (var ingredient in recipes[i])
            {
                Debug.Log(ingredient.Ingrediente);
                //Get instance script give it the ingredient image to display on top of the previous one
                //instance.GetComponentInChildren<TextMeshProUGUI>().text += ingredient.name.ToString() + " ";
                GameObject sprite = new GameObject("sprite");
                sprite.transform.SetParent(instanceRectTransform.GetChild(0).GetChild(0).transform);
                sprite.AddComponent<LayoutElement>();
                sprite.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                foreach (IngredientsScriptableObject ingredientScriptableObject in currentMenu)
                {
                    Debug.Log("Ingr:" +ingredientScriptableObject.IngredientName);
                    if (ingredientScriptableObject.ID == ingredient.Ingrediente)
                    {
                        Debug.Log("HOLA");
                        sprite.AddComponent<Image>().sprite = ingredientScriptableObject.AlternativeSprite && j == recipes[i].Count - 1 ?
                            ingredientScriptableObject.AlternativeSprite : ingredientScriptableObject.Sprite;
                        j++;
                        break;
                    }
                }               
                //if (ingredient.Rarity == IngredientRarity.core) { continue; }

                //Give to the script the code object ignoring breads
                GameObject prefab = new GameObject("code");
                prefab.transform.SetParent(instanceRectTransform.GetChild(1).GetChild(0).transform);
                prefab.transform.SetSiblingIndex(Random.Range(0, instanceRectTransform.childCount));
                prefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                prefab.AddComponent<LayoutElement>();

                Image prefabImage = prefab.AddComponent<Image>();
                //refabImage.sprite = Codes[codes[ingredient]];


            }
            //instance.GetComponentInChildren<TextMeshProUGUI>().text += "] ";
        }

    }
}
