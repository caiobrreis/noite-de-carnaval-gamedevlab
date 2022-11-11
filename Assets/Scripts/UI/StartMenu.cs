using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void StartButton()
    {
        SceneManager.LoadScene("Map");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
