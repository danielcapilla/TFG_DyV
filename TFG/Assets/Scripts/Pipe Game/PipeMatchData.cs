using System;
using System.Collections.Generic;

/// <summary>Estado de una tile en el grid.</summary>
[Serializable]
public class PipeTileState
{
    public int col;
    public int row;
    public string shapeType;
    public int rotation;
    public bool isLocked;
}

/// <summary>Snapshot del estado del grid en un momento concreto.</summary>
[Serializable]
public class PipeGridSnapshot
{
    public List<PipeTileState> tiles = new();
}

/// <summary>Un intento de completar el circuito (cada vez que se pulsa la palanca).</summary>
[Serializable]
public class PipeAttempt
{
    public int attemptNumber;
    public bool success;
    public PipeGridSnapshot gridState;
}

/// <summary>Datos de una ronda completa.</summary>
[Serializable]
public class PipeRound
{
    public int roundNumber;
    public float timeSeconds;
    public PipeGridSnapshot initialState;
    public List<PipeAttempt> attempts = new();
}

/// <summary>Datos completos de una partida de Pipe Game.</summary>
[Serializable]
public class PipeMatchData
{
    public string date;
    public string teacherCode;
    public string classCode;
    public int gridColumns;
    public int gridRows;
    public string difficulty;
    public float matchDuration;
    public bool infiniteTime;
    public int maxRounds;
    public List<PipeRound> rounds = new();
}
