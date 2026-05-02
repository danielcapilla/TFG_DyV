/// <summary>
/// Interfaz que deben implementar los paneles de configuracion de cada minijuego.
/// MinigameSelectorBehaviour llama a Setup() al instanciar el panel.
/// </summary>
public interface IConfigPanel
{
    void Setup(GameConfigBaseSO config);
}
