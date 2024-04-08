using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommandSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    public TextMeshProUGUI [] textMeshProUGUI;

    private void Start()
    {
        textMeshProUGUI = GetComponentsInChildren<TextMeshProUGUI>();
    }

    public void SpawnRecipes(List<List<IngredientsScriptableObject>> recipes )
    {

        for (int i = 0; i < recipes.Count; i++)
        {
            GameObject instance = Instantiate(panel);
            instance.transform.SetParent(this.transform, false);
            RectTransform instanceRectTransform = instance.GetComponent<RectTransform>();
            //Para mover cosas en canvas usar anchoredPosition!!!!
            instanceRectTransform.anchoredPosition = new Vector3(instanceRectTransform.sizeDelta.x*i,0f,0f);
            //textMeshProUGUI[i].text += "[ ";
            //foreach (IngredientsScriptableObject ingredient in recipes[i])
            //{
            //    textMeshProUGUI[i].text += ingredient.name.ToString() + " ";
            //}
            //textMeshProUGUI[i].text += "] ";
        }
    }
}
