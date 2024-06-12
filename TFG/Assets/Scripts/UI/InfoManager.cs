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
    [SerializeField]
    private GameObject hamburguesaCompletada;
    int i = 0;

    private void OnEnable()
    {
        i = 0;
        SpawnRecipes(filtersBehaviour.match.HamburguesasEjemplo);
        SpawnOrder(filtersBehaviour.match.HamburguesasCorrectas[i]);
    }
    private void OnDisable()
    {
        foreach (Transform child in hamburguesasIzq.transform)
        {
            Destroy(child.gameObject);
        }
        DestroyOrder();
    }
    public void NextOrder()
    {
        DestroyOrder();
        i++;
        SpawnOrder(filtersBehaviour.match.HamburguesasCorrectas[i % filtersBehaviour.match.HamburguesasCorrectas.Count]);
    }
    public void PrevOrder()
    {
        DestroyOrder();
        i--;
        if (i < 0) i = filtersBehaviour.match.HamburguesasCorrectas.Count - 1;
        SpawnOrder(filtersBehaviour.match.HamburguesasCorrectas[i % filtersBehaviour.match.HamburguesasCorrectas.Count]);
    }
    private void DestroyOrder()
    {
        foreach (Transform child in hamburguesaCompletada.transform.GetChild(0).GetChild(0).transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in hamburguesaCompletada.transform.GetChild(1).GetChild(0).transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void SpawnOrder(List<BurguerJSONCreator.Ingredientes> order)
    {
        //instance.transform.SetParent(this.transform.GetChild(0), false);
        //Para mover cosas en canvas usar anchoredPosition!!!!

        int j = 0;
        foreach (var ingredient in order)
        {
            GameObject sprite = new GameObject("sprite");
            sprite.transform.SetParent(hamburguesaCompletada.transform.GetChild(0).GetChild(0).transform);
            sprite.AddComponent<LayoutElement>();
            sprite.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            foreach (IngredientsScriptableObject ingredientScriptableObject in currentMenu)
            {
                if (ingredientScriptableObject.ID == ingredient.Ingrediente)
                {
                    sprite.AddComponent<Image>().sprite = ingredientScriptableObject.AlternativeSprite && j == order.Count - 1 ?
                        ingredientScriptableObject.AlternativeSprite : ingredientScriptableObject.Sprite;
                    j++;
                    break;
                }
            }
            if (ingredient.Codigo == -1) { continue; }

            //Give to the script the code object ignoring breads
            GameObject prefab = new GameObject("code");
            prefab.transform.SetParent(hamburguesaCompletada.transform.GetChild(1).GetChild(0).transform);
            //prefab.transform.SetSiblingIndex(Random.Range(0, instanceRectTransform.childCount));
            prefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            prefab.AddComponent<LayoutElement>();

            Image prefabImage = prefab.AddComponent<Image>();
            prefabImage.sprite = Codes[ingredient.Codigo];

        }
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

            int j = 0;
            foreach (var ingredient in recipes[i])
            {
                GameObject sprite = new GameObject("sprite");
                sprite.transform.SetParent(instanceRectTransform.GetChild(0).GetChild(0).transform);
                sprite.AddComponent<LayoutElement>();
                sprite.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                foreach (IngredientsScriptableObject ingredientScriptableObject in currentMenu)
                {
                    if (ingredientScriptableObject.ID == ingredient.Ingrediente)
                    {
                        sprite.AddComponent<Image>().sprite = ingredientScriptableObject.AlternativeSprite && j == recipes[i].Count - 1 ?
                            ingredientScriptableObject.AlternativeSprite : ingredientScriptableObject.Sprite;
                        j++;
                        break;
                    }
                }               
                if (ingredient.Codigo == -1) { continue; }

                //Give to the script the code object ignoring breads
                GameObject prefab = new GameObject("code");
                prefab.transform.SetParent(instanceRectTransform.GetChild(1).GetChild(0).transform);
                prefab.transform.SetSiblingIndex(Random.Range(0, instanceRectTransform.childCount));
                prefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                prefab.AddComponent<LayoutElement>();

                Image prefabImage = prefab.AddComponent<Image>();
                Debug.Log(ingredient.Codigo);
                prefabImage.sprite = Codes[ingredient.Codigo];
            }
        }

    }
}
