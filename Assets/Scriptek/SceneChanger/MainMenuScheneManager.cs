using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneManager : MonoBehaviour
{
    public void Play()
    {
        SceneTransitionManager.Instance.LoadScene("Garage",
            SceneTransitionManager.TransitionType.LoadingScreen);
    }

    public void Settings()
    {
        SceneTransitionManager.Instance.LoadScene("Settings");

    }

    public void Exit()
    {
        Application.Quit();
    }
}