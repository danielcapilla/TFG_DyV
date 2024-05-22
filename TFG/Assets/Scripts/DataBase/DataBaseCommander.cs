using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class DataBaseCommander : MonoBehaviour
{
    string Username = "TFGVCDC";
    string Password = "2024TFGminijuegos";
    string url = "https://tfvj.etsii.urjc.es/";
    string UserDataTable = "UserData";

    string contentType = "application/json";

    static string token = "";

    public void DataBaseLogin()
    {
        string json = CreateLoginJSON();
        StartCoroutine(SendTokenRequest(json));
    }

    string CreateLoginJSON()
    {
        string json = $@"{{
            ""username"":""{Username}"",
            ""password"":""{Password}""
        }}";

        return json;
    }

    string CreateSaveJSON()
    {
        //Construye JSON para la petici�n REST         
        string json = $@"{{
            ""username"":""{Username}"",
            ""password"":""{Password}"",
            ""table"":""{UserDataTable}"",
            ""data"": {{
                ""Username"": ""{PlayerData.Name}"",
                ""Age"": {PlayerData.Age},
                ""Gender"": ""{PlayerData.Gender}"",
                ""Role"": ""{PlayerData.Role}"",
                ""ClassCode"": ""{PlayerData.ClassCode}"",
                ""ProfilePicID"": {PlayerData.ProfilePicID}
            }}
        }}";

        return json;
    }

    string CreateSelectJSON(string username)
    {
        string json = $@"{{
            ""username"":""{Username}"",
            ""password"":""{Password}"",
            ""table"":""{UserDataTable}"",
            ""filter"": {{
                ""Username"": ""{username}""
            }}
        }}";

        return json;
    }

    string CreateUpdateJSON()
    {
        string json = $@"{{
            ""username"":""{Username}"",
            ""token"":""{token}"",
            ""table"":""{UserDataTable}"",
            ""data"": {{
                ""ProfilePicID"": {PlayerData.ProfilePicID}
            }},
            ""filter"": {{
                ""Username"": ""{PlayerData.Name}""
            }}
        }}";

        return json;
    }

    public void RegisterUser(Action<int> FailCallback)
    {
        string json = CreateSaveJSON();
        StartCoroutine(RegisterUserDB(json, FailCallback));
    }

    IEnumerator RegisterUserDB(string save, Action<int> FailCallback)
    {
        Debug.Log(save);
        using (UnityWebRequest www = UnityWebRequest.Post(url + "insert", save, contentType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                print("Error: " + www.error);
                Debug.Log("Error info: " + www.result);
                print("Respuesta: " + www.downloadHandler.text);

                if (www.result == UnityWebRequest.Result.ProtocolError)
                {
                    FailCallback(0);
                }
                else
                {
                    FailCallback(1);
                }
            }
            else
            {
                print("Respuesta: " + www.downloadHandler.text);
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    public void LoadGame(string Username, Action<int> FailCallback)
    {
        string json = CreateSelectJSON(Username);
        StartCoroutine(LoadGameDB(json, FailCallback));
    }

    IEnumerator LoadGameDB(string filter, Action<int> FailCallback)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(url + "get", filter, contentType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                print("Error: " + www.error);
                FailCallback(1);
            }
            else
            {
                print("Respuesta: " + www.downloadHandler.text);

                Response response = JsonUtility.FromJson<Response>(www.downloadHandler.text);
                if (response.data.Length > 0)
                {
                    ResponseData data = response.data[0];

                    PlayerData.Name = data.Username;
                    PlayerData.Age = data.Age;
                    PlayerData.Gender = data.Gender;
                    PlayerData.Role = data.Role;
                    PlayerData.ClassCode = data.ClassCode;
                    PlayerData.ProfilePicID = data.ProfilePicID;

                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    FailCallback(0);
                }
            }
        }
    }


    public void UpdateGame()
    {
        string json = CreateUpdateJSON();
        StartCoroutine(UpdateGameDB(json));
    }

    IEnumerator UpdateGameDB(string save)
    {
        Debug.Log("Token mandado: " + token);
        //Update previous save
        using (UnityWebRequest www = UnityWebRequest.Post(url + "rest/update", save, contentType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                print("Error: " + www.error);
                print("Error details" + www.downloadHandler.text);
            }
            else
            {
                print("Respuesta: " + www.downloadHandler.text);
            }
        }
    }

    IEnumerator SendTokenRequest(string data)
    {


        using (UnityWebRequest www = UnityWebRequest.Post(url + "rest/login", data, contentType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                print("Error: " + www.error);
            }
            else
            {
                print("Respuesta: " + www.downloadHandler.text);
                LoginDBResponse response = JsonUtility.FromJson<LoginDBResponse>(www.downloadHandler.text);
                token = response.token;
                print("Token: " + token);
            }
        }
    }

    [System.Serializable]
    private class Response
    {
        public string result;
        public ResponseData[] data;
    }

    [System.Serializable]
    private class ResponseData
    {
        public string Username;
        public int Age;
        public string Gender;
        public string Role;
        public string ClassCode;
        public int ProfilePicID = 0;
    }

    [System.Serializable]
    private class LoginDBResponse
    {
        public string result;
        public string token;
        public string until;
    }
}
