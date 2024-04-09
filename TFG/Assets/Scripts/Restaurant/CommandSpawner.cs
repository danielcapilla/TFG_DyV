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
    public List<Sprite> Codes;

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
            instance.transform.SetParent(this.transform, false);
            RectTransform instanceRectTransform = instance.GetComponent<RectTransform>();
            //Para mover cosas en canvas usar anchoredPosition!!!!
            instanceRectTransform.anchoredPosition = new Vector3(instanceRectTransform.sizeDelta.x*i,0f,0f);

            instance.GetComponentInChildren<TextMeshProUGUI>().text += "[ ";
            foreach (IngredientsScriptableObject ingredient in recipes[i])
            {
                if(ingredient.Rarity == IngredientRarity.core) { continue;  }
                GameObject prefab = new GameObject("code");
                prefab.transform.SetParent(instanceRectTransform.GetChild(1).transform);
                prefab.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
                prefab.AddComponent<LayoutElement>().preferredWidth = 0;

                Image prefabImage = prefab.AddComponent<Image>();
                prefabImage.sprite = Codes[codes[ingredient]];
                
                instance.GetComponentInChildren<TextMeshProUGUI>().text += ingredient.name.ToString() + " ";
            }
            instance.GetComponentInChildren<TextMeshProUGUI>().text += "] ";
        }

        
    }
}
