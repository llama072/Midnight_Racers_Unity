using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsSceneManager : MonoBehaviour
{
    public void Exit()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
