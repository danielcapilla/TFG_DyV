using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HamburgerOrderScript : MonoBehaviour
{
    [SerializeField] 
    private TeamMenager teamManager;
    private Button previousButton;
    [SerializeField]
    private GameObject hamburgerOrder;
    [SerializeField]
    private GameObject namesOrder;
    public void ShowHamburgerInfo()
    {
        hamburgerOrder.SetActive(true);
        namesOrder.SetActive(true);
        if (previousButton != null)
        {
            previousButton.interactable = true;
        }
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        int groupNumber = (int.Parse(clickedButton.GetComponentInChildren<TextMeshProUGUI>().text)) - 1;
        TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamManager.teams[groupNumber];
        clickedButton.interactable = false;
        previousButton = clickedButton;
        int j = 0;
        foreach (IngredientBehaviour ingredientIB in teamInfo.order)
        {
            GameObject sprite = new GameObject("sprite");
            sprite.transform.SetParent(hamburgerOrder.transform);
            sprite.AddComponent<LayoutElement>();
            sprite.AddComponent<Image>().sprite = ingredientIB.ingredient.AlternativeSprite && ingredientIB == teamInfo.order.Last<IngredientBehaviour>()? 
                ingredientIB.ingredient.AlternativeSprite : ingredientIB.ingredient.Sprite;
            GameObject name = new GameObject("name");
            name.transform.SetParent(namesOrder.transform);
            name.AddComponent<LayoutElement>();
            name.AddComponent<TextMeshProUGUI>().text = teamInfo.namesOfOrder[j].ToString();
            j++;
        }
    }
}
