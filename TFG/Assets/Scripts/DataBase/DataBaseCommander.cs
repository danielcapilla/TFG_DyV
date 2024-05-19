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

    //TODO capture and display duplicate username error
    //TODO capture and display username doesnt exist in db at login

    string CreateSaveJSON()
    {
        //Construye JSON para la petición REST         
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


    IEnumerator SendPostRequest(string data)
    {


        using (UnityWebRequest www = UnityWebRequest.Post(url + "insert", data, contentType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                print("Error: " + www.error);
            }
            else
            {
                print("Respuesta: " + www.downloadHandler.text);
            }
        }
    }

    public void RegisterUser()
    {
        string json = CreateSaveJSON();
        StartCoroutine(RegisterUserDB(json));
    }

    IEnumerator RegisterUserDB(string save)
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
            }
            else
            {
                print("Respuesta: " + www.downloadHandler.text);
                SceneManager.LoadScene("MainMenu");
            }
        }
    }


    public void LoadGame(string Username)
    {
        string json = CreateSelectJSON(Username);
        StartCoroutine(LoadGameDB(json));
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

    IEnumerator LoadGameDB(string filter)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(url + "get", filter, contentType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                print("Error: " + www.error);
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
}
