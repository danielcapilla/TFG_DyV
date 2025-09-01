using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationBox : MonoBehaviour
{
    public TextMeshProUGUI titleText;  // Asigna aquí el texto del título (TextMeshPro)
    public TextMeshProUGUI messageText;  // Asigna aquí el texto de alerta (TextMeshPro)
    public Button closeButton;      // Asigna aquí el botón de cerrar
    public Image background;

    NotificationConfig config;

    private void Start()
    {
        // Asigna el método CloseAlert al botón de cerrar
        closeButton.onClick.AddListener(CloseNotification);
    }

    // Método estático para mostrar el mensaje de alerta
    public void ApplyConfig(NotificationConfig config)
    {
        this.config = config;
        // Title & message
        titleText.text = config.Title;
        messageText.text = config.Message;

        // Background
        background.color = config.BackgroundColor;

        // Auto-close
        if (config.Duration > 0)
            StartCoroutine(AutoClose(config.Duration));
    }

    private IEnumerator AutoClose(float time)
    {
        yield return new WaitForSeconds(time);
        CloseNotification();
    }

    // Método privado para cerrar el mensaje de alerta
    private void CloseNotification()
    {
        config?.OnClose?.Invoke();  // Llama a la acción de cierre si está configurada
        Destroy(gameObject);  // Destruye el panel de alerta al cerrar
    }
}

[System.Serializable]
public class NotificationConfig
{
    public string Title = "Notification";
    public string Message = "";
    public Color BackgroundColor;
    public float Duration = -1; // -1 = do not auto-close

    public Action OnClose; // Action to call when the notification is closed
}
