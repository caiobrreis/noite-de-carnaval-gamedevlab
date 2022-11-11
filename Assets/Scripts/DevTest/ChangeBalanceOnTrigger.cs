using UnityEngine;

public class ChangeBalanceOnTrigger : MonoBehaviour
{
	public int amount;
	public bool increaseBalance;
	public bool destroy;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			var player = other.GetComponent<Player>();

			if (increaseBalance)
			{
				player.IncreaseBalance(amount);
			}
			else
			{
				player.DecreaseBalance(amount);
			}

			if (destroy)
			{
				Destroy(gameObject);
			}
		}
	}
}