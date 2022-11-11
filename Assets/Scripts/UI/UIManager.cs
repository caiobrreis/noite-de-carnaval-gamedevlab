using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    private Player player;

    private int previousHealth;
    private int previousBalance;

    private void Awake() 
    {
        player = FindObjectOfType<Player>();
    }

    private void Start() 
    {
        SetupHealthUI();
        SetupBalanceUI();
        
        previousHealth = player.CurrentHealth;
        previousBalance = player.Balance;
    }

    private void Update() 
    {
        if (ValueChanged(previousHealth, player.CurrentHealth))
        {
            UpdateHealthUI();
        }

        if (ValueChanged(previousBalance, player.Balance))
        {
            UpdateBalanceUI();
        }

        previousHealth = player.CurrentHealth;
        previousBalance = player.Balance;
    }

    private bool ValueChanged(int old, int new_)
	{
		if (old != new_) return true;
        return false;
	}

    #region Health
    private void SetupHealthUI()
    {
        GameManager.manager.data.healthContainer.gameObject.SetActive(true);

        for (int i = 0; i < player.CurrentHealth; i++)
        {
            GameObject heart = Instantiate(GameManager.manager.data.heartPrefab, GameManager.manager.data.healthContainer);
        }

        for (int i = player.CurrentHealth; i < player.maxHealth; i++)
        {
            GameObject heart = Instantiate(GameManager.manager.data.heartPrefab, GameManager.manager.data.healthContainer);
            heart.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

	private void UpdateHealthUI()
	{
		if (HealthDecreased())
        {
            OnHealthDecreased();
        }
        else
        {
            OnHealthIncreased();
        }
	}

	private void OnHealthIncreased()
	{
        for (int i = previousHealth; i < player.CurrentHealth; i++)
        {
		    GameManager.manager.data.healthContainer.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
        }
	}

	private void OnHealthDecreased()
	{
		for (int i = player.CurrentHealth; i < previousHealth; i++)
        {
		    GameManager.manager.data.healthContainer.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
        }
	}

	private bool HealthDecreased()
	{
		if (player.CurrentHealth < previousHealth) return true;

        return false;
	}
    #endregion

    #region Balance
    private void SetupBalanceUI()
    {
        var coinContainer = GameManager.manager.data.coinContainer;
        
        coinContainer.gameObject.SetActive(true);
        coinContainer.GetComponentInChildren<TextMeshProUGUI>().text = player.Balance.ToString();
    }

    private void UpdateBalanceUI()
	{
        GameManager.manager.data.coinContainer.GetComponentInChildren<TextMeshProUGUI>().text = player.Balance.ToString();
	}
    #endregion
}
