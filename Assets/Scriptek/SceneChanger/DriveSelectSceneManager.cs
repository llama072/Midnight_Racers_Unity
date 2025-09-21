using UnityEngine;
using UnityEngine.SceneManagement;

public class DriveSelectSceneManager : MonoBehaviour
{
    public void DayDrive()
    {
        SceneManager.LoadScene("DayDrive");
    }
    public void NightDrive() 
    {
        SceneManager.LoadScene("NightDrive");
    }
    public void Exit()
    {
        SceneManager.LoadScene("Garage");
    }
}
