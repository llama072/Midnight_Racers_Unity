using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[System.Serializable]
public class LoginResponse
{
    public bool success;
    public int userId;
    public string message;
}

[System.Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[System.Serializable]
public class SaveScoreRequest
{
    public int userId;
    public int score;
}

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager instance;
    public static int LoggedInUserId = -1;

    private string baseUrl = "http://127.0.0.1:3000";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public delegate void LoginCallback(bool success);

    public void Login(string username, string password, LoginCallback callback)
    {
        StartCoroutine(LoginRoutine(username, password, callback));
    }

    IEnumerator LoginRoutine(string username, string password, LoginCallback callback)
    {
        LoginRequest data = new LoginRequest
        {
            username = username,
            password = password
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("LOGIN REQUEST: " + json);

        UnityWebRequest request = new UnityWebRequest(baseUrl + "/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        Debug.Log("RAW RESPONSE: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
        {
            LoginResponse res = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

            if (res != null && res.success)
            {
                LoggedInUserId = res.userId;
                Debug.Log("LOGIN OK 🔥 ID: " + LoggedInUserId);
                callback(true);
            }
            else
            {
                Debug.LogWarning("LOGIN FAIL: " + (res != null ? res.message : "null"));
                callback(false);
            }
        }
        else
        {
            Debug.LogError("NETWORK ERROR: " + request.error);
            callback(false);
        }
    }

    public void SaveScore(int score)
    {
        if (LoggedInUserId == -1)
        {
            Debug.LogWarning("Nincs login, nem mentünk.");
            return;
        }

        StartCoroutine(SaveScoreRoutine(LoggedInUserId, score));
    }

    IEnumerator SaveScoreRoutine(int userId, int score)
    {
        SaveScoreRequest data = new SaveScoreRequest
        {
            userId = userId,
            score = score
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("SAVE REQUEST: " + json);

        UnityWebRequest request = new UnityWebRequest(baseUrl + "/save-score", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        Debug.Log("SAVE RESPONSE: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("MENTVE ✅");
        }
        else
        {
            Debug.LogError("SAVE ERROR: " + request.error);
        }
    }
}