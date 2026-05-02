using UnityEngine;

[CreateAssetMenu(fileName = "RestaurantGameConfig", menuName = "Game Config/Restaurant")]
public class RestaurantGameConfigSO : GameConfigBaseSO
{
    [Header("Restaurante")]
    public int recipesPerTeam = 3;
}
