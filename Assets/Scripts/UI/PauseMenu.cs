using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [HideInInspector] public bool isMenuOpen;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
		{
			ToggleMenu();
		}
	}

	private void ToggleMenu()
	{
        isMenuOpen = !isMenuOpen;

		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(isMenuOpen);
		}
	}

	public void Continue()
    {
        ToggleMenu();
    }

    public void Restart()
    {
        SceneManager.LoadScene("Map");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
