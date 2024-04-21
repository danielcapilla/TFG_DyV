using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamInfo", menuName = "Teams/TeamInfo")]
public abstract class TeamInfo : ScriptableObject
{
    public int ID;
    public List<ulong> integrantes = new List<ulong>();
    public int Puntuacion = 0;

    public virtual TeamInfo clone() 
    {
        return (TeamInfo)ScriptableObject.CreateInstance<TeamInfo>(); ;
    }
}
