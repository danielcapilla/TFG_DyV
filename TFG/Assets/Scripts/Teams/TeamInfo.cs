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

    /// <summary>
    /// Cada subclase debe sobreescribir este metodo para clonar su tipo concreto.
    /// </summary>
    public abstract TeamInfo Clone();
}
