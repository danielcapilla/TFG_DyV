using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserDatabaseService : DatabaseConnection
{
    private const string Table = "UserData";
    [Serializable] private class SaveRequest   { public string username; public string password; public string table; public UserData data; }
    [Serializable] private class SelectRequest { public string username; public string password; public string table; public UserFilter filter; }
    [Serializable] private class UpdateRequest { public string username; public string token; public string table; public UpdateData data; public UserFilter filter; }
    [Serializable] private class UserData   { public string Username; public int Age; public string Gender; public string Role; public string ClassCode; public int ProfilePicID; }
    [Serializable] private class UpdateData { public int ProfilePicID; }
    [Serializable] private class UserFilter { public string Username; }
    [Serializable] private class ProfileResponse { public string result; public ProfileData[] data; }
    [Serializable] private class ProfileData { public string Username; public int Age; public string Gender; public string Role; public string ClassCode; public int ProfilePicID; }

    private void Start() => Login();

    public void RegisterUser(Action<int> onFail)
    {
        var req = new SaveRequest { username = AppUser, password = AppPass, table = Table,
            data = new UserData { Username = PlayerData.Name, Age = PlayerData.Age, Gender = PlayerData.Gender,
                Role = PlayerData.Role, ClassCode = PlayerData.ClassCode, ProfilePicID = PlayerData.ProfilePicID } };
        Post("insert", JsonUtility.ToJson(req), (ok, body) =>
        { if (ok) SceneManager.LoadScene("MainMenu"); else onFail?.Invoke(body.Contains("409") ? 0 : 1); });
    }

    public void LoadUser(string username, Action<int> onFail)
    {
        var req = new SelectRequest { username = AppUser, password = AppPass, table = Table,
            filter = new UserFilter { Username = username } };
        Post("get", JsonUtility.ToJson(req), (ok, body) =>
        {
            if (!ok) { onFail?.Invoke(1); return; }
            var res = JsonUtility.FromJson<ProfileResponse>(body);
            if (res.data == null || res.data.Length == 0) { onFail?.Invoke(0); return; }
            var d = res.data[0];
            PlayerData.Name = d.Username; PlayerData.Age = d.Age; PlayerData.Gender = d.Gender;
            PlayerData.Role = d.Role; PlayerData.ClassCode = d.ClassCode; PlayerData.ProfilePicID = d.ProfilePicID;
            SceneManager.LoadScene("MainMenu");
        });
    }

    public void UpdateProfilePic(Action onComplete = null)
    {
        var req = new UpdateRequest { username = AppUser, token = Token, table = Table,
            data = new UpdateData { ProfilePicID = PlayerData.ProfilePicID },
            filter = new UserFilter { Username = PlayerData.Name } };
        Post("rest/update", JsonUtility.ToJson(req), (ok, _) => onComplete?.Invoke());
    }
}
