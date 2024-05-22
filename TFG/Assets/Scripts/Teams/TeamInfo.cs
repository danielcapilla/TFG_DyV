using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamInfo", menuName = "Teams/TeamInfo")]
public abstract class TeamInfo : ScriptableObject
{
    public int ID;
    public List<ulong> integrantes = new List<ulong>();
    public int Puntuacion = 0;
    public Action<int> onPuntuacionChanged;

    public virtual TeamInfo Clone()
    {
        return (TeamInfo)ScriptableObject.CreateInstance<TeamInfo>(); ;
    }
}
