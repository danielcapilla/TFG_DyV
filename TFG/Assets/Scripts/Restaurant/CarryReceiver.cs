/// <summary>
/// Implementa esta interfaz en cualquier lugar del entorno que pueda RECIBIR
/// un ICarryObject: mesas, papeleras, huecos de baldosa, estaciones de entrega...
///
/// El receptor tiene total autonomia sobre que hacer con el objeto:
/// colocarlo en su sitio, destruirlo, evaluarlo, etc.
/// </summary>
public interface ICarryReceiver
{
    /// <summary>
    /// Devuelve true si este receptor acepta el objeto en este momento.
    /// Se llama antes de Receive para poder dar feedback visual al jugador.
    /// </summary>
    bool CanReceive(ICarryObject carryObject, PlayerCarry carrier);

    /// <summary>
    /// El jugador entrega el objeto. El receptor decide completamente que hacer.
    /// Devuelve true si acepta (el jugador pierde el objeto).
    /// Devuelve false si rechaza (el jugador conserva el objeto).
    /// </summary>
    bool Receive(ICarryObject carryObject, PlayerCarry carrier);
}
