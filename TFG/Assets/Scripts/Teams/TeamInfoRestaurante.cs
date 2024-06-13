using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamInfoRestaurante", menuName = "Teams/TeamInfoRestaurante")]
public class TeamInfoRestaurante : TeamInfo
{
    public int idOrder;
    public List<DeliveredBurguerInfo> Burguers = new();
    public Action<int> OnIdOrderChange;
    public override TeamInfo Clone()
    {
        return (TeamInfo)ScriptableObject.CreateInstance<TeamInfoRestaurante>();
    }
}

public class DeliveredBurguerInfo
{
    public int idOrder;
    public List<IngredientBehaviour> burguer;
}
