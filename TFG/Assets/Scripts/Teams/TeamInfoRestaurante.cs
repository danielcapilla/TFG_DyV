using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamInfoRestaurante", menuName = "Teams/TeamInfoRestaurante")]
public class TeamInfoRestaurante : TeamInfo
{
    public int idOrder;
    public List<IngredientBehaviour> order = new List<IngredientBehaviour>();
    public List<FixedString64Bytes> namesOfOrder = new List<FixedString64Bytes>();
    public Action<int> OnIdOrderChange;
    public override TeamInfo Clone()
    {
        return (TeamInfo)ScriptableObject.CreateInstance<TeamInfoRestaurante>();
    }
}
