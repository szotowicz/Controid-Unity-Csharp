using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        Debug.Log("New game");
        SceneManager.LoadScene("NewGame");
    }

    public void ConnectController()
    {
        Debug.Log("ConnectController");
        SceneManager.LoadScene("ConnectController");
    }

    public void Options()
    {
        Debug.Log("options");
        SceneManager.LoadScene("Options");
    }

    public void AboutGame()
    {
        Debug.Log(" about game");
        SceneManager.LoadScene("AboutGame");
    }

    public void Exit()
    {
        Debug.Log("exit");
        Application.Quit();
    }

    public void ReturnToMenu()
    {
        Debug.Log("return to menu");
        SceneManager.LoadScene("MainMenu");
    }
}