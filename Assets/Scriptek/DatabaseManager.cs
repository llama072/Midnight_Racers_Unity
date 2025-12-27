using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

// Segédosztály a szerver válaszának értelmezéséhez
[System.Serializable]
public class LoginResponse
{
    public bool success;
    public int userId;
    public string message;
}

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager instance;
    public static int LoggedInUserId = -1; // -1 jelenti, hogy senki nincs belépve
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

    // --- LOGIN FUNKCIÓ ---
    public delegate void LoginCallback(bool success);

    public void Login(string username, string password, LoginCallback callback)
    {
        StartCoroutine(LoginRoutine(username, password, callback));
    }

    IEnumerator LoginRoutine(string username, string password, LoginCallback callback)
    {
        // Csak nevet és jelszót küldünk a szervernek
        string json = "{\"username\":\"" + username + "\", \"password\":\"" + password + "\"}";
        var request = new UnityWebRequest(baseUrl + "/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // A szerver válaszát (pl. {"success":true, "userId":1}) objektummá alakítjuk
            LoginResponse res = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

            if (res.success)
            {
                LoggedInUserId = res.userId; // Itt mentjük el az ID-t a Stats mentéshez!
                Debug.Log("Sikeres belépés! ID: " + LoggedInUserId);
                callback(true);
            }
            else
            {
                Debug.LogWarning("Belépési hiba: " + res.message);
                callback(false);
            }
        }
        else
        {
            Debug.LogError("Hálózati hiba: " + request.error);
            callback(false);
        }
    }

    // --- MENTÉS FUNKCIÓ ---
    public void SaveScore(int score)
    {
        if (LoggedInUserId == -1)
        {
            Debug.LogWarning("Nincs belépett felhasználó, a pontszám nem lesz mentve.");
            return;
        }
        StartCoroutine(SaveScoreRoutine(LoggedInUserId, score));
    }

    IEnumerator SaveScoreRoutine(int userId, int score)
    {
        string json = "{\"userId\":" + userId + ", \"score\":" + score + "}";
        var request = new UnityWebRequest(baseUrl + "/save-score", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) Debug.Log("Pontszám sikeresen mentve az adatbázisba!");
        else Debug.LogError("Mentési hiba: " + request.error);
    }
}