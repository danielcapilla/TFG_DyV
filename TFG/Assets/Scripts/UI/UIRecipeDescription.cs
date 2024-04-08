using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRecipeDescription : MonoBehaviour
{
    [SerializeField]
    private Image recipeImage;
    [SerializeField]
    private Image codeImage;
    [SerializeField]
    private TMP_Text title;
    [SerializeField]
    private TMP_Text description;

    private void Awake()
    {
        //ResetDescription();
    }

    //public void ResetDescription()
    //{
    //    itemImage.gameObject.SetActive(false);
    //    title.text = "";
    //    description.text = "";
    //}

    //public void SetDescription(Sprite sprite, string itemName,
    //    string itemDescription)
    //{
    //    itemImage.gameObject.SetActive(true);
    //    itemImage.sprite = sprite;
    //    title.text = itemName;
    //    description.text = itemDescription;
    //}
}
