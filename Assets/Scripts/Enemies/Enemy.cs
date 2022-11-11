using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    private int health;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int damage;
    [SerializeField] protected float speed;
    [SerializeField] protected float detectionRange;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected Player playerScript;
    protected Transform player;
	protected LayerMask playerLayer;
	private float pushDistance = 1f;
	private float pushSpeed = 10f;

    protected string currentAnimState;
    protected bool inHitConfusion;

	public int Health { 
        get => health; 
        set {
            health = Mathf.Clamp(value, 0, maxHealth);
            if (health == 0)
            {
                Die();
            }
        }
    }

	protected virtual void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerLayer = 1 << LayerMask.NameToLayer("Player");
        health = maxHealth;
        
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerScript = player.GetComponent<Player>();
    }

    protected Vector3 GetDirectionToPlayerCollider()
    {
        return (playerScript.PlayerColCenter - transform.position).normalized;
    }

    protected void MoveToDirection(Vector3 dir)
	{
        rb.velocity = dir * speed;
	}

	protected bool isDetectingPlayer()
	{
		return Vector3.Distance(player.position, transform.position) <= detectionRange;
	}

    private IEnumerator GetPushed(Vector3 pusherPosition)
    {
        Vector3 dir = (pusherPosition - transform.position).normalized;
        Vector3 target = transform.position - (dir * pushDistance);

        while (Vector2.Distance(transform.position, target) > 0.001f)
        {
            float step = pushSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, target, step);
            yield return new WaitForEndOfFrame();
        }
    }

    protected void HitPlayer()
    {
        playerScript.Hit(damage, transform.position);
    }

    private void StopConfusion()
    {
        inHitConfusion = false;
    }

    protected void ChangeAnimationState(string newState)
    {
        if (currentAnimState == newState) return;

        anim.Play(newState);

        currentAnimState = newState;
    }

    public void Hit(int amount, Vector3 pusherPosition=default(Vector3))
    {
        Health -= amount;

        if (pusherPosition != Vector3.zero)
        {
            StartCoroutine(GetPushed(pusherPosition));
        }

        inHitConfusion = true;
        Invoke("StopConfusion", 0.5f);
        rb.velocity = Vector2.zero;
    }

    protected void Heal(int amount)
    {
        Health += amount;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
