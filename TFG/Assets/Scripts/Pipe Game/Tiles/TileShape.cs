using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipo de forma de una loseta de tuberia.
/// Cada forma define qué lados tienen apertura (Norte, Sur, Este, Oeste).
/// Norte = +Z, Sur = -Z, Este = +X, Oeste = -X (en espacio local de la loseta).
/// </summary>
public enum TileShapeType
{
    Straight,   // recta: N-S
    Curve,      // curva: N-E
    TSplit,     // T:     N-E-O
    Cross,      // cruz:  N-S-E-O
}

[System.Flags]
public enum TileDirection
{
    None  = 0,
    North = 1 << 0,
    South = 1 << 1,
    East  = 1 << 2,
    West  = 1 << 3,
}

public static class TileShapeData
{
    /// <summary>Devuelve las aperturas de una forma antes de rotarla.</summary>
    public static TileDirection GetOpenings(TileShapeType shape)
    {
        switch (shape)
        {
            case TileShapeType.Straight: return TileDirection.North | TileDirection.South;
            case TileShapeType.Curve:    return TileDirection.North | TileDirection.East;
            case TileShapeType.TSplit:   return TileDirection.North | TileDirection.East | TileDirection.West;
            case TileShapeType.Cross:    return TileDirection.North | TileDirection.South | TileDirection.East | TileDirection.West;
            default: return TileDirection.None;
        }
    }

    /// <summary>
    /// Rota las aperturas segun los grados (multiples de 90).
    /// </summary>
    public static TileDirection Rotate(TileDirection openings, int degrees)
    {
        int steps = ((degrees / 90) % 4 + 4) % 4;
        TileDirection result = openings;
        for (int i = 0; i < steps; i++)
            result = RotateOnce(result);
        return result;
    }

    private static TileDirection RotateOnce(TileDirection d)
    {
        // 90 grados horario: N->E->S->W->N
        TileDirection r = TileDirection.None;
        if ((d & TileDirection.North) != 0) r |= TileDirection.East;
        if ((d & TileDirection.East)  != 0) r |= TileDirection.South;
        if ((d & TileDirection.South) != 0) r |= TileDirection.West;
        if ((d & TileDirection.West)  != 0) r |= TileDirection.North;
        return r;
    }

    /// <summary>La direccion opuesta a la dada.</summary>
    public static TileDirection Opposite(TileDirection d)
    {
        switch (d)
        {
            case TileDirection.North: return TileDirection.South;
            case TileDirection.South: return TileDirection.North;
            case TileDirection.East:  return TileDirection.West;
            case TileDirection.West:  return TileDirection.East;
            default: return TileDirection.None;
        }
    }
}
