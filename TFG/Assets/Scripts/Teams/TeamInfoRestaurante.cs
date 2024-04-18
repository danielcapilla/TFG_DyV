using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamInfoRestaurante", menuName = "Teams/TeamInfoRestaurante")]
public class TeamInfoRestaurante : TeamInfo
{
    public int idOrder;

    public override TeamInfo clone()
    {
        return (TeamInfo)ScriptableObject.CreateInstance<TeamInfoRestaurante>();
    }
}
