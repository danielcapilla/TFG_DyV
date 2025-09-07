using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamInfoChicken", menuName = "Teams/TeamInfoChicken")]
public class TeamInfoChicken : TeamInfo
{
    //public int idOrder;
    //public List<DeliveredBurguerInfo> Burguers = new();
    //public Action<int> OnIdOrderChange;
    public bool spawnedPlayer = false;
    public override TeamInfo Clone()
    {
        return (TeamInfo)ScriptableObject.CreateInstance<TeamInfoChicken>();
    }
}
