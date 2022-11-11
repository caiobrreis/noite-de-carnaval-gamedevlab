using UnityEngine;

public class ChangeHealthOnTrigger : MonoBehaviour
{
	public int amount;
	public bool doDamage;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			var player = other.GetComponent<Player>();

			if (doDamage)
			{
				player.Hit(amount);
			}
			else
			{
				player.Heal(amount);
			}
		}
	}
}