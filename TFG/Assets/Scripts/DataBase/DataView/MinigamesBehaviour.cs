using UnityEngine;

public class MinigamesBehaviour : MonoBehaviour
{
    [SerializeField] private FiltersBehaviour filters;

    public void SelectRestaurant()
    {
        if (filters == null) return;
        filters.SetGameType(FiltersBehaviour.GameType.Restaurant);
    }

    public void SelectChicken()
    {
        if (filters == null) return;
        filters.SetGameType(FiltersBehaviour.GameType.Chicken);
    }

    public void SelectPipe()
    {
        if (filters == null) return;
        filters.SetGameType(FiltersBehaviour.GameType.Pipe);
    }
}
