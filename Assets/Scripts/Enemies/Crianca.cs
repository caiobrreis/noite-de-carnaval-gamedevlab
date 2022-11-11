using System.Collections;
using UnityEngine;

public class Crianca : Enemy
{
	private LineRenderer lineRenderer;
	private Collider2D col;
	[SerializeField] private Transform gunPivot;
	[SerializeField] private Transform gunFirePoint;
	[SerializeField] private SpriteRenderer targetCrosshairRenderer;
	[SerializeField] private Material dotMaterial;
	[SerializeField] private Material waterMaterial;

	private State currentState = State.idle;
	private enum State
	{
		idle,
		aiming,
		shooting,
		resting,
	}

	[SerializeField] private float restingDuration = 1f;
	[SerializeField] private float aimingDuration = 1f;
	[SerializeField] private float shootingDuration = 2f;
	private float restingTimer;
	private float aimingTimer;
	private float shootingTimer;

	[SerializeField] private float waterJetSpeed = 15f;
	private float waterJetDistance { get => waterJetSpeed * shootingDuration; }

	const string ANIM_IDLE = "Idle";
    const string ANIM_HIT = "Hit";

	protected override void Awake()
	{
		base.Awake();

		col = GetComponent<Collider2D>();
		lineRenderer = GetComponentInChildren<LineRenderer>();
		lineRenderer.material.mainTextureScale = new Vector2(1f / lineRenderer.startWidth, 1f);
		lineRenderer.enabled = false;
		lineRenderer.positionCount = 2;
		lineRenderer.startWidth = 0.3f;
	}

	private void Start() 
	{
		aimingTimer = aimingDuration;
		restingTimer = restingDuration;
	}

    private void Update()
	{
		if (inHitConfusion)
		{
			ChangeAnimationState(ANIM_HIT);
			return;
		}
		else
		{
			ChangeAnimationState(ANIM_IDLE);
		}

		if (currentState == State.resting)
		{
			restingTimer -= Time.deltaTime;

			if (restingTimer > 0f) return;

			restingTimer = restingDuration;
			currentState = State.idle;
		}

		if (isDetectingPlayer())
		{
			if (currentState == State.idle)
			{
				// TODO: girar em direção ao jogador

				InitAimTrajectory();
				currentState = State.aiming;
			}
		}

		if (currentState == State.aiming)
		{
			aimingTimer -= Time.deltaTime;
			UpdateAimTrajectory();

			if (aimingTimer <= 0f)
			{
				aimingTimer = aimingDuration;
				DisableAimTrajectory();
				StartCoroutine(Shot());
				currentState = State.shooting;
			}
		}
	}

	private IEnumerator Shot()
	{
		InitShot();

		bool hitPlayer = false;
		// Vector3 shotDirection = GetDirectionToPlayerCollider();
		Vector3 shotDirection = (playerScript.PlayerColCenter - gunFirePoint.position).normalized;
		Vector3 currentShotPosition = gunFirePoint.position;
		Vector3 waterJetEndPosition = gunFirePoint.position + shotDirection * waterJetDistance;
		var shootingAngle = GetGunAngle();

		shootingTimer = shootingDuration;
		while (shootingTimer > 0)
		{
			shootingTimer -= Time.deltaTime;

            gunPivot.rotation = Quaternion.Euler(0.0f, 0.0f, -shootingAngle);

			RaycastHit2D hit = Physics2D.Raycast(gunFirePoint.position, shotDirection, waterJetDistance, ~( 1 << gameObject.layer));


			if (hit.collider)
			{
				var lengthOriginToShot = (gunFirePoint.position - currentShotPosition).magnitude;
				var lengthOriginToHit = (hit.point - (Vector2)gunFirePoint.position).magnitude;

				if (lengthOriginToShot >= lengthOriginToHit)
				{
					if (1 << hit.collider.gameObject.layer == playerLayer.value)
					{
						if (!hitPlayer)
						{
							HitPlayer();
							hitPlayer = true;
						}

						currentShotPosition = hit.point;
					}
				}
				else
				{
					currentShotPosition = Vector2.MoveTowards(currentShotPosition, hit.point, waterJetSpeed * Time.deltaTime);
				}
			}
			else
			{
				currentShotPosition = Vector2.MoveTowards(currentShotPosition, waterJetEndPosition, waterJetSpeed * Time.deltaTime);
			}

			lineRenderer.SetPosition(0, gunFirePoint.position);
			lineRenderer.SetPosition(1, currentShotPosition);
			lineRenderer.material.mainTextureOffset += new Vector2(-Time.deltaTime * waterJetSpeed, 0f);

			yield return new WaitForEndOfFrame();
		}

		lineRenderer.enabled = false;
		currentState = State.resting;
	}

	private float GetGunAngle()
	{
		var gunSR = gunPivot.GetChild(0).GetComponent<SpriteRenderer>();

		// var dir = GetDirectionToPlayerCollider();
		var dir = (playerScript.PlayerColCenter - gunFirePoint.position).normalized;
		float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

		if (angle < 0f)
		{
			gunSR.flipX = true;
		}
		else
		{
			gunSR.flipX = false;
		}

        return angle;
	}

	private void InitShot()
	{
		lineRenderer.enabled = true;
		lineRenderer.material = waterMaterial;
		lineRenderer.material.mainTextureScale = Vector2.one;
	}

	private void DisableAimTrajectory()
	{
		lineRenderer.enabled = false;
		targetCrosshairRenderer.enabled = false;
	}

	private void InitAimTrajectory()
	{
		targetCrosshairRenderer.enabled = true;
		lineRenderer.enabled = true;
		lineRenderer.material = dotMaterial;
		lineRenderer.material.mainTextureScale = new Vector2(1f / lineRenderer.startWidth, 1f);
	}

	private void UpdateAimTrajectory()
	{
		lineRenderer.SetPosition(0, playerScript.PlayerColCenter);
		lineRenderer.SetPosition(1, gunFirePoint.position);
		targetCrosshairRenderer.transform.position = playerScript.PlayerColCenter;

		var shootingAngle = GetGunAngle();
		gunPivot.rotation = Quaternion.Euler(0.0f, 0.0f, -shootingAngle);
	}
}
