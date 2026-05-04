using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Base para todos los servicios de BD.
/// Gestiona el token de forma centralizada — si no hay token, hace login primero
/// y encola las peticiones hasta que el login termine.
/// </summary>
public abstract class DatabaseConnection : MonoBehaviour
{
    [SerializeField] protected DatabaseCredentialsSO credentials;

    protected static string Token = "";
    private static bool isLoggingIn = false;
    private static readonly Queue<Action> pendingRequests = new Queue<Action>();

    protected string Url     => credentials.url;
    protected string AppUser => credentials.appUsername;
    protected string AppPass => credentials.appPassword;
    protected string ContentType = "application/json";

    [Serializable] private class LoginRequest  { public string username; public string password; }
    [Serializable] private class LoginResponse { public string result; public string token; public string until; }

    // ── Login centralizado ────────────────────────────────────────────────────

    protected void Login(Action onComplete = null)
    {
        if (!string.IsNullOrEmpty(Token)) { onComplete?.Invoke(); return; }
        if (isLoggingIn) { if (onComplete != null) pendingRequests.Enqueue(onComplete); return; }

        isLoggingIn = true;
        if (onComplete != null) pendingRequests.Enqueue(onComplete);

        var req  = new LoginRequest { username = AppUser, password = AppPass };
        StartCoroutine(SendRequest("rest/login", JsonUtility.ToJson(req), (ok, body) =>
        {
            if (ok)
            {
                var res = JsonUtility.FromJson<LoginResponse>(body);
                Token = res.token;
                Debug.Log("[DB] Token obtenido hasta " + res.until);
            }
            else
            {
                Debug.LogWarning("[DB] Login fallido: " + body);
            }
            isLoggingIn = false;
            // Ejecutar todas las peticiones encoladas
            while (pendingRequests.Count > 0)
                pendingRequests.Dequeue()?.Invoke();
        }));
    }

    private void Start() => Login();

    // ── Peticion POST centralizada ────────────────────────────────────────────

    protected void Post(string endpoint, string json, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(Token))
        {
            // Sin token: hacer login primero y encolar la peticion
            Login(() => StartCoroutine(SendRequest(endpoint, json, callback)));
        }
        else
        {
            StartCoroutine(SendRequest(endpoint, json, callback));
        }
    }

    private IEnumerator SendRequest(string endpoint, string json, Action<bool, string> callback)
    {
        Debug.Log($"[DB] POST {Url + endpoint}");
        using (var www = UnityWebRequest.Post(Url + endpoint, json, ContentType))
        {
            yield return www.SendWebRequest();
            bool ok = www.result == UnityWebRequest.Result.Success;
            string body = www.downloadHandler.text;
            Debug.Log($"[DB] Response {endpoint}: ok={ok} status={www.responseCode} body={body.Substring(0, Mathf.Min(300, body.Length))}");
            if (!ok) Debug.LogWarning($"[DB] {endpoint} error: {www.error}");
            callback(ok, body);
        }
    }
}
