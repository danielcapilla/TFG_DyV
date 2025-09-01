using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationBuilder
{
    private NotificationConfig config;
    private NotificationBox box;

    public NotificationBuilder(NotificationConfig initialConfig)
    {
        config = initialConfig;
        box = NotificationManager.CreateNotification(config); // Implicit show
    }

    public NotificationBuilder SetTitle(string title)
    {
        config.Title = title;
        box.ApplyConfig(config);
        return this;
    }

    public NotificationBuilder SetMessage(string message)
    {
        config.Message = message;
        box.ApplyConfig(config);
        return this;
    }

    public NotificationBuilder SetBackgroundColor(Color color)
    {
        config.BackgroundColor = color;
        box.ApplyConfig(config);
        return this;
    }

    public NotificationBuilder SetDuration(float duration)
    {
        config.Duration = duration;
        box.ApplyConfig(config);
        return this;
    }

    public NotificationBuilder OnClose(Action callback)
    {
        config.OnClose += callback;
        return this;
    }
}
