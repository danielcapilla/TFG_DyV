/// <summary>
/// Interfaz para cualquier sistema que pueda otorgar puntos.
/// Implementar en DeliveryStation, PipeConnectionChecker, etc.
/// Score.cs se suscribe automaticamente a todos los IScoreEvent de la escena.
/// </summary>
public interface IScoreEvent
{
    /// <summary>Se dispara cuando se consigue un punto. El int es la cantidad de puntos ganados.</summary>
    event System.Action<int> OnScorePoint;
}
