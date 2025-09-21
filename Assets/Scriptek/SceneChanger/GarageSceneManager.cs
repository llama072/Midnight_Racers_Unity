using UnityEngine;
using UnityEngine.SceneManagement;

public class GarageSceneManager : MonoBehaviour
{
    public void Exit()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void Drive()
    {
        SceneManager.LoadScene("Drive Select");
    }
}
