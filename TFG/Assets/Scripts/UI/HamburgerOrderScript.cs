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
    private GameObject ingridientTarjetita;
    public void ShowHamburgerInfo()
    {
        hamburgerOrder.SetActive(true);
        if (previousButton != null)
        {
            previousButton.interactable = true;
        }
        Button clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        int groupNumber = (int.Parse(clickedButton.GetComponentInChildren<TextMeshProUGUI>().text)) - 1;
        TeamInfoRestaurante teamInfo = (TeamInfoRestaurante)teamManager.teams[groupNumber];
        foreach (Transform child in hamburgerOrder.transform)
        {
            Destroy(child.gameObject);
        }
        clickedButton.interactable = false;
        previousButton = clickedButton;
        if (teamInfo.Burguers.LastOrDefault().burguer == null) return;
        List<IngredientBehaviour> order = teamInfo.Burguers.LastOrDefault().burguer;
        foreach (IngredientBehaviour ingredientIB in order)
        {
            GameObject tarjetita = Instantiate(ingridientTarjetita);
            tarjetita.transform.SetParent(hamburgerOrder.transform);
            tarjetita.GetComponentInChildren<Image>().sprite = ingredientIB.ingredient.AlternativeSprite && ingredientIB == order.LastOrDefault<IngredientBehaviour>() ?
                ingredientIB.ingredient.AlternativeSprite : ingredientIB.ingredient.Sprite;
            tarjetita.GetComponentInChildren<TextMeshProUGUI>().text = ingredientIB.playerName.ToString();
        }
    }
}
