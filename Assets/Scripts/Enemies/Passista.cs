using UnityEngine;

public class Passista : Enemy
{
	[SerializeField] private Transform attackLocation;
	[SerializeField] private float attackRadius;
	
	[Space(5)]
	
	[SerializeField] private Sprite attackingFrame;
	[SerializeField] private Sprite notAttackingFrame;
	
	[Space(5)]
	
	[SerializeField] private float changeFrameInterval = 2f;

	private SpriteRenderer sr;
	private float frameTimer;
	private bool isInAttackFrame = false;
	const string ANIM_HIT = "Hit";

	protected override void Awake() 
	{
		base.Awake();
		sr = GetComponent<SpriteRenderer>();
	}

	private void Start() 
	{
		frameTimer = changeFrameInterval;
	}

    private void Update()
	{
		if (inHitConfusion)
		{
			anim.enabled = true;
			ChangeAnimationState(ANIM_HIT);
			return;
		}
		else
		{
			if (anim.enabled)
			{
				anim.enabled = false;
				
				if (isInAttackFrame)
				{
					sr.sprite = attackingFrame;
				}
				else
				{
					sr.sprite = notAttackingFrame;
				}
			}
		}

		if (Vector2.Distance(attackLocation.position, playerScript.PlayerColCenter) > attackRadius && isDetectingPlayer())
		{
			MoveToDirection(GetDirectionToPlayerCollider());
		}
		else
		{
			rb.velocity = Vector2.zero;
		}

		UpdateFrameTimer();

		if (frameTimer == 0f)
		{
			OnStateChangedToAttack();
		}
		else if (frameTimer == changeFrameInterval)
		{
			OnStateChangedToNotAttack();
		}
	}

	private void OnStateChangedToNotAttack()
	{
		isInAttackFrame = false;
		sr.sprite = notAttackingFrame;
	}

	private void OnStateChangedToAttack()
	{
		isInAttackFrame = true;
		sr.sprite = attackingFrame;
		MeleeAttack();
	}

	private void UpdateFrameTimer()
	{
		if (isInAttackFrame)
		{
			frameTimer += Time.deltaTime;
		}
		else
		{
			frameTimer -= Time.deltaTime;
		}

		frameTimer = Mathf.Clamp(frameTimer, 0f, changeFrameInterval);
	}

	private void MeleeAttack()
    {
        Collider2D playerCol = Physics2D.OverlapCircle(attackLocation.position, attackRadius, playerLayer);

		if (playerCol != null)
		{
			HitPlayer();
		}
    }
}
