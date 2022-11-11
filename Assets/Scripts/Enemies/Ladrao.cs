using System;
using System.Collections;
using UnityEngine;

public class Ladrao : Enemy
{
    private enum State
    {
        idle,
        runningToPlayer,
        runningFromPlayer
    }

    private State currentState = State.idle;

    private bool carryingCoin = false;

    Plane[] planes;
    Collider2D objCollider;
    SpriteRenderer sr;
    bool facingRight = true;
    private SpriteRenderer handSR;
    
    [SerializeField] private float showHandDuration = 0.5f;
    [SerializeField] private float stealRange;
    [SerializeField] private Sprite punchSprite;
    [SerializeField] private Sprite stealSprite;

    const string ANIM_MOVING = "Moving";
    const string ANIM_HIT = "Hit";

	protected override void Awake() 
    {
        base.Awake();
        objCollider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        handSR = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void Update() 
    {
        planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        // Seen by the camera
        if (GeometryUtility.TestPlanesAABB(planes, objCollider.bounds))
		{
            if (inHitConfusion)
            {
                ChangeAnimationState(ANIM_HIT);
                return;
            }

			Flip(rb.velocity.x);

            ChangeAnimationState(ANIM_MOVING);

			if (currentState == State.idle)
			{
				currentState = State.runningToPlayer;
			}

			if (currentState == State.runningToPlayer)
			{
				MoveToDirection(GetDirectionToPlayerCollider());

				if (Vector3.Distance(transform.position, player.position) < stealRange)
				{
					if (playerScript.Balance > 0)
					{
                        StartCoroutine(ShowHand(stealSprite));
						playerScript.DecreaseBalance(1);
						carryingCoin = true;
					}
					else
					{
                        StartCoroutine(ShowHand(punchSprite));
						HitPlayer();
					}

					currentState = State.runningFromPlayer;
				}
			}

			if (currentState == State.runningFromPlayer)
			{
				MoveToDirection(GetDirectionToPlayerCollider() * -1);
			}
		}
		else
        {
            if (currentState == State.runningFromPlayer)
            {
                Destroy(gameObject);
            }
        }
    }

	private IEnumerator ShowHand(Sprite sprite)
	{
        handSR.enabled = true;
		handSR.sprite = sprite;

        yield return new WaitForSeconds(showHandDuration);

        handSR.enabled = false;
	}

	private void Flip(float value)
	{
		if (value > 0)
		{
			facingRight = false;
		}
		else
		{
			facingRight = true;
		}

		sr.flipX = facingRight;
	}

	protected override void Die()
    {
        if (carryingCoin)
        {
            playerScript.IncreaseBalance(1);
        }

        base.Die();
    }
}
