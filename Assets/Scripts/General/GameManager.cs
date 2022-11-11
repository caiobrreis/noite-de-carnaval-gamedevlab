using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager manager { get; private set; }
    public SceneData data;
	private int playerHealth;
	private int playerBalance;

    private void Awake()
	{
        // Keep it across scene loads
		DontDestroyOnLoad(this);

		if (manager != null && manager != this)
		{
			Destroy(this.gameObject);
			return;
		}
		else
		{
			manager = this;
		}

		SaveData();
	}

	private void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		data = FindObjectOfType<SceneData>();

		LoadData();
	}

    private void SaveData()
	{
		var player = FindObjectOfType<Player>();
		playerHealth = player.CurrentHealth;
		playerBalance = player.Balance;
	}

	private void LoadData()
	{
		var player = FindObjectOfType<Player>();
		player.CurrentHealth = playerHealth;
		player.Balance = playerBalance;
	}

    public void SaveDataAndLoadScene(string sceneName)
    {
        SaveData();
        SceneManager.LoadScene(sceneName);
    }
}
