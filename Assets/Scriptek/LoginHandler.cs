using UnityEngine;
using TMPro;

public class LoginHandler : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public void OnLoginClick()
    {
        string user = usernameInput.text;
        string pass = passwordInput.text;

        // Meghívjuk a DatabaseManagert a beírt adatokkal
        DatabaseManager.instance.Login(user, pass, (success) => {
            if (success)
            {
                Debug.Log("Belépve! Mehet a menet.");
                // Itt töltheted be a következő pályát
                // SceneManager.LoadScene("Drive Select");
            }
            else
            {
                Debug.LogError("Hibás felhasználónév vagy jelszó!");
            }
        });
    }
}