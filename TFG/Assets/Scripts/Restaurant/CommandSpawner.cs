using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private GameObject panelOrder;
    [SerializeField] 
    public List<Sprite> Codes;
    private GameObject instance;
    
    //[SerializeField]
    //private UIInventoryItem itemPrefab;

    //[SerializeField]
    //private RectTransform contentPanel;

    //[SerializeField]
    //private UIInventoryDescription itemDescription;

    //[SerializeField]
    //private MouseFollower mouseFollower;

    public void SpawnRecipes(List<List<IngredientsScriptableObject>> recipes, Dictionary<IngredientsScriptableObject, int> codes)
    {

        for (int i = 0; i < recipes.Count; i++)
        {
            GameObject instance = Instantiate(panel);
            instance.transform.SetParent(this.transform.GetChild(0), false);
            RectTransform instanceRectTransform = instance.GetComponent<RectTransform>();
            //Para mover cosas en canvas usar anchoredPosition!!!!
            instanceRectTransform.anchoredPosition = new Vector3(instanceRectTransform.sizeDelta.x*i,0f,0f);

            //instance.GetComponentInChildren<TextMeshProUGUI>().text += "[ ";
            int j = 0;
            foreach (IngredientsScriptableObject ingredient in recipes[i])
            {
                //Get instance script give it the ingredient image to display on top of the previous one
                //instance.GetComponentInChildren<TextMeshProUGUI>().text += ingredient.name.ToString() + " ";
                GameObject sprite = new GameObject("sprite");
                sprite.transform.SetParent(instanceRectTransform.GetChild(0).GetChild(0).transform);
                sprite.AddComponent<LayoutElement>();
                sprite.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                sprite.AddComponent<Image>().sprite = ingredient.AlternativeSprite && j == recipes[i].Count-1 ? ingredient.AlternativeSprite  : ingredient.Sprite;
                j++;
                if (ingredient.Rarity == IngredientRarity.core) { continue;  }

                //Give to the script the code object ignoring breads
                GameObject prefab = new GameObject("code");
                prefab.transform.SetParent(instanceRectTransform.GetChild(1).GetChild(0).transform);
                prefab.transform.SetSiblingIndex(Random.Range(0, instanceRectTransform.childCount));
                prefab.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
                prefab.AddComponent<LayoutElement>();

                Image prefabImage = prefab.AddComponent<Image>();
                prefabImage.sprite = Codes[codes[ingredient]];
                
                
            }
            //instance.GetComponentInChildren<TextMeshProUGUI>().text += "] ";
        }
        
    }

    public void SpawnOrder(Dictionary<IngredientsScriptableObject, int> codes, List<IngredientsScriptableObject> order)
    {

        if (instance != null) Destroy(instance);
        instance = Instantiate(panelOrder);
        instance.transform.SetParent(this.transform.GetChild(0), false);
        RectTransform instanceRectTransform = instance.GetComponent<RectTransform>();
        //Para mover cosas en canvas usar anchoredPosition!!!!
        instanceRectTransform.anchoredPosition = new Vector3(0f , 0f, 0f);
        foreach (IngredientsScriptableObject ingredient in order)
        {

            if (ingredient.Rarity == IngredientRarity.core) { continue; }

            //Give to the script the code object ignoring breads
            GameObject prefab = new GameObject("code");
            prefab.transform.SetParent(instanceRectTransform.GetChild(1).transform);
            prefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            prefab.AddComponent<LayoutElement>();

            Image prefabImage = prefab.AddComponent<Image>();
            prefabImage.sprite = Codes[codes[ingredient]];


        }

    }
}
