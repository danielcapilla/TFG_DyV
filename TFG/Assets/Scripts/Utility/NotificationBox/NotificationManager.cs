using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    #region Singleton
    public static NotificationManager Instance 
    {
        get 
        {
            if(_instance != null) 
            {
                return _instance;
            }

            _instance = FindObjectOfType<NotificationManager>();

            if (_instance != null)
            {
                return _instance;
            }

            CreateNewInstance();
            return _instance;
        }
    }

    private static void CreateNewInstance()
    {
        NotificationManager notificationManagerPrefab = Resources.Load<NotificationManager>("NotificationBox/NotificationManagerPrefab");
        _instance = Instantiate(notificationManagerPrefab);
        _instance.NotificationPrefab = Resources.Load<GameObject>("NotificationBox/NotificationBoxPrefab");
    }

    private static NotificationManager _instance;
    #endregion

    [SerializeField]
    GameObject NotificationPrefab; // TODO: Maybe a pool
    private void Awake()
    {
        // Implementación del Singleton
        if (Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public NotificationBuilder ShowNotification(string message)
    {
        return new NotificationBuilder(new NotificationConfig 
        { 
            Message = message 
        });
    }

    #region Presets
    public NotificationBuilder ShowErrorNotification(string message)
    {
        return new NotificationBuilder(new NotificationConfig
        {
            Title = "Error",
            Message = message,
            BackgroundColor = Color.red,
            Duration = 5f
        });
    }

    public NotificationBuilder ShowSuccessNotification(string message)
    {
        return new NotificationBuilder(new NotificationConfig
        {
            Title = "Success",
            Message = message,
            BackgroundColor = Color.green
        });
    }

    public NotificationBuilder ShowWarningNotification(string message)
    {
        return new NotificationBuilder(new NotificationConfig
        {
            Title = "Warning",
            Message = message,
            BackgroundColor = Color.yellow
        });
    }

    public NotificationBuilder ShowInfoNotification(string message)
    {
        return new NotificationBuilder(new NotificationConfig
        {
            Title = "Info",
            Message = message,
            BackgroundColor = Color.cyan
        });
    }
    #endregion

    // Internal method to actually create a notification
    internal static NotificationBox CreateNotification(NotificationConfig config)
    {
        if (Instance == null)
        {
            Debug.LogError("No NotificationManager instance found in the scene.");
            return null;
        }

        var go = Instantiate(Instance.NotificationPrefab, Instance.transform);
        var box = go.GetComponent<NotificationBox>();
        box.ApplyConfig(config);
        return box;
    }
}
