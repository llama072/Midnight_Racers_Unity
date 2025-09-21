using UnityEngine;
using UnityEngine.SceneManagement;

public class NightDriveSceneManager : MonoBehaviour
{
    public void Exit()
    {
        SceneManager.LoadScene("Garage");
    }
}
