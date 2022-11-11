using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D playerCol;

    [Header("Movement")]
    public float movementSpeed = 5f;
    private Vector2 movement;
    private float pushDistance = 1f;
    private float pushSpeed = 10f;
    private bool canBeControlled = true;
	public bool CanBeControlled {
        get { return canBeControlled; }
        set
        {
            canBeControlled = value;
            rb.velocity = Vector2.zero;
        }
    }

    [Header("Health")]
    public int maxHealth;
	private int currentHealth;
	public int CurrentHealth { 
        get { return currentHealth; } 
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            if (currentHealth == 0)
            {
                OnDeath();
            }
        }
    }

	[Header("Melee Attack")]
	[SerializeField] private int meleeDamage = 1;
	[SerializeField] private float meleeDuration = 0.5f;
    [SerializeField] private GameObject armPivotUp;
    [SerializeField] private GameObject armPivotLeft;
    [SerializeField] private GameObject armPivotRight;
    [SerializeField] private GameObject armPivotDown;
    [SerializeField] private float armAnimMaxRange = 2f;

	[Header("Ranged Attack")]
    [SerializeField] private int rangedAtkDamage = 1;
    [SerializeField] private float rangedAtkDuration = 3f;
    [SerializeField] private float rangedAtkRange = 5f;
    [SerializeField] private Transform rangedAtkObject;

    [Header("Ultimate")]
    [SerializeField] private int ultimateDamage = 1;
    [SerializeField] private float ultimateDuration = 6f;
    [SerializeField] private float ultimateDamageFrequency = 1f;
    [SerializeField] private float ultimateRadius = 1f;
    [SerializeField] private float ultimateCooldown = 3f;
    [SerializeField] private GameObject ultimateParticles;
    private float ultimateTimer = 0f;

    [Header("Dash")]
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashSpeed = 25f;
    private float dashTimer = 0f;

    [Header("General")]
    [SerializeField] private LayerMask enemiesLayer;
	
    // Balance
	private int balance;
    public int Balance { 
        get { return balance; }
        set { balance = Mathf.Clamp(value, 0, 999999); }
    }

    // Animation
    private string currentAnimState;

    const string ANIM_IDLE_UP = "Idle_Back";
    const string ANIM_IDLE_LEFT = "Idle_Left";
    const string ANIM_IDLE_RIGHT = "Idle_Right";
    const string ANIM_IDLE_DOWN = "Idle_Front";

    const string ANIM_IDLE_UP_NOARM = "Idle_Back_NoArm";
    const string ANIM_IDLE_LEFT_NOARM = "Idle_Left";
    const string ANIM_IDLE_RIGHT_NOARM = "Idle_Right_NoArm";
    const string ANIM_IDLE_DOWN_NOARM = "Idle_Front_NoArm";

    const string ANIM_MOVE_UP = "Walk_Back";
    const string ANIM_MOVE_LEFT = "Walk_Left";
    const string ANIM_MOVE_RIGHT = "Walk_Right";
    const string ANIM_MOVE_DOWN = "Walk_Front";

    const string ANIM_MOVE_UP_NOARM = "Walk_Back_NoArm";
    const string ANIM_MOVE_LEFT_NOARM = "Walk_Left";
    const string ANIM_MOVE_RIGHT_NOARM = "Walk_Right_NoArm";
    const string ANIM_MOVE_DOWN_NOARM = "Walk_Front_NoArm";

    const string ANIM_HIT = "Hit";
    const string ANIM_DANCE = "Dance";

    // Misc
    public Vector3 PlayerColCenter { get => playerCol.bounds.center; }
    private bool isMeleeAttacking;
    private bool isRangedAttacking;
    private bool isUltimateAttacking;
    private bool isMovingToTarget;
    private bool inHitConfusion;
	private bool inDanceBattle;
    private float movementAngle;

	private bool canUltimate { get => ultimateTimer <= 0f; }
    private bool canDash { get => dashTimer <= 0f; }
    private bool isMoving { get => movement != Vector2.zero; }
	private bool lookingUp { get => movementAngle >= 292.5f || movementAngle < 22.5f; }
	private bool lookingLeft { get => movementAngle >= 202.5f && movementAngle < 292.5f; }
	private bool lookingRight { get => movementAngle >= 22.5f && movementAngle < 112.5f; }
	private bool lookingDown { get => movementAngle >= 112.5f && movementAngle < 202.5f; }

	void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCol = GetComponent<Collider2D>();

        CurrentHealth = maxHealth;
    }

    public void StartDancing()
    {
        ChangeAnimationState(ANIM_DANCE);
        inDanceBattle = true;
    }

    public void StopDancing()
    {
        ChangeAnimationState(ANIM_IDLE_DOWN);
        inDanceBattle = false;
    }

    void Update()
	{
        dashTimer -= Time.deltaTime;
        ultimateTimer -= Time.deltaTime;

        if (inHitConfusion)
		{
			ChangeAnimationState(ANIM_HIT);
			return;
		}

		if (!canBeControlled) return;

		movement = GetMovementInput();

        if (isMoving)
		{
			movementAngle = GetMovementAngle();
		}

		if (!isMeleeAttacking)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(MeleeAttackAnimation());
                StartCoroutine(MeleeAttack());
            }
        }

        if (!isRangedAttacking)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(RangedAttackAnimation());
                StartCoroutine(RangedAttack());
            }
        }

        if (canUltimate)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(Ultimate());
                ultimateTimer = ultimateCooldown;
            }
        }

        if (canDash)
        {
            if (Input.GetMouseButtonDown(1))
			{
				Vector3 target = transform.position + GetMouseDirection() * dashDistance;
				StartCoroutine(MovePlayerTo(target, dashSpeed));
				dashTimer = dashCooldown;
			}
		}

        Animate();
	}

	void FixedUpdate() 
    {
        if (!canBeControlled) return;

        if (!isMeleeAttacking)
        {
            rb.velocity = movement.normalized * movementSpeed;
        }
        else
        {
            rb.velocity = movement.normalized * (movementSpeed / 2f);
        }
    }

    #region Animation
    private IEnumerator MeleeAttackAnimation()
    {
        // if already animating, return
        if (isMeleeAttacking)
        {
            yield break;
        }

        isMeleeAttacking = true;
        GameObject armPivot = GetArmPivot();
        armPivot.SetActive(true);
        Transform arm = armPivot.transform.GetChild(0);
        Vector3 armOriginalPos = arm.localPosition;
        Vector3 relativeMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        Vector3 mouseDir = relativeMousePos.normalized;
        var timer = meleeDuration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            float rotationZ = Mathf.Atan2(relativeMousePos.y, relativeMousePos.x) * Mathf.Rad2Deg;
            armPivot.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);

            if (timer >= meleeDuration / 2)
            {
                arm.position += mouseDir * Time.deltaTime * (armAnimMaxRange / (meleeDuration / 2));
            }
            else
            {
                arm.position -= mouseDir * Time.deltaTime * (armAnimMaxRange / (meleeDuration / 2));
            }

            yield return new WaitForEndOfFrame();
        }

        isMeleeAttacking = false;
        arm.localPosition = armOriginalPos;
        armPivot.SetActive(false);
    }

    private IEnumerator RangedAttackAnimation()
	{
		// if already animating, return
        if (isRangedAttacking)
        {
            yield break;
        }

        isRangedAttacking = true;
        rangedAtkObject.gameObject.SetActive(true);
        rangedAtkObject.position = transform.position;
        Vector3 mouseDir = GetMouseDirection();
        var timer = rangedAtkDuration;

        while (true)
        {
            timer -= Time.deltaTime;

            if (timer >= rangedAtkDuration / 2)
            {
                rangedAtkObject.position += mouseDir * Time.deltaTime * (rangedAtkRange / (rangedAtkDuration / 2));
            }
            else
            {
                var playerDir = (rangedAtkObject.position - transform.position).normalized;
                rangedAtkObject.position -= playerDir * Time.deltaTime * (rangedAtkRange / (rangedAtkDuration / 2));

                if (Vector2.Distance(rangedAtkObject.position, transform.position) < 0.01f)
                {
                    rangedAtkObject.gameObject.SetActive(false);
                    yield return new WaitForSeconds(timer);
                    break;
                }
            }

            yield return new WaitForEndOfFrame();
        }

        isRangedAttacking = false;
        rangedAtkObject.gameObject.SetActive(false);
	}

    private void StopConfusion()
    {
        inHitConfusion = false;

        if (inDanceBattle)
        {
            ChangeAnimationState(ANIM_DANCE);
        }
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentAnimState == newState) return;

        anim.Play(newState);

        currentAnimState = newState;
    }

	private void Animate()
	{
		if (!isMeleeAttacking)
		{
			if (!isMoving)
			{
				if (lookingUp)
				{
					ChangeAnimationState(ANIM_IDLE_UP);
				}
				else if (lookingRight)
				{
					ChangeAnimationState(ANIM_IDLE_RIGHT);
				}
				else if (lookingLeft)
				{
					ChangeAnimationState(ANIM_IDLE_LEFT);
				}
				else if (lookingDown)
				{
					ChangeAnimationState(ANIM_IDLE_DOWN);
				}
			}
			else
			{
				if (lookingUp)
				{
					ChangeAnimationState(ANIM_MOVE_UP);
				}
				else if (lookingRight)
				{
					ChangeAnimationState(ANIM_MOVE_RIGHT);
				}
				else if (lookingLeft)
				{
					ChangeAnimationState(ANIM_MOVE_LEFT);
				}
				else if (lookingDown)
				{
					ChangeAnimationState(ANIM_MOVE_DOWN);
				}
			}
		}
		else
		{
			if (!isMoving)
			{
				if (lookingUp)
				{
					ChangeAnimationState(ANIM_IDLE_UP_NOARM);
				}
				else if (lookingRight)
				{
					ChangeAnimationState(ANIM_IDLE_RIGHT_NOARM);
				}
				else if (lookingLeft)
				{
					ChangeAnimationState(ANIM_IDLE_LEFT_NOARM);
				}
				else if (lookingDown)
				{
					ChangeAnimationState(ANIM_IDLE_DOWN_NOARM);
				}
			}
			else
			{
				if (lookingUp)
				{
					ChangeAnimationState(ANIM_MOVE_UP_NOARM);
				}
				else if (lookingRight)
				{
					ChangeAnimationState(ANIM_MOVE_RIGHT_NOARM);
				}
				else if (lookingLeft)
				{
					ChangeAnimationState(ANIM_MOVE_LEFT_NOARM);
				}
				else if (lookingDown)
				{
					ChangeAnimationState(ANIM_MOVE_DOWN_NOARM);
				}
			}
		}
	}
    #endregion

    #region Movement
    private Vector2 GetMovementInput()
	{
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
	}

    private float GetMovementAngle()
	{
		float angle = Mathf.Atan2(movement.normalized.x, movement.normalized.y) * Mathf.Rad2Deg;

		if (angle < 0f)
		{
			angle += 360f;
		}

        return angle;
	}

    // Move player to target and return true when finished
    public bool MoveToWithReturn(Vector2 target, float speedMultiplier=1f)
    {
        var step = movementSpeed * speedMultiplier * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        if (Vector3.Distance(transform.position, target) < 0.001f) {
            return true;
        }

        return false;
    }

    private IEnumerator MovePlayerTo(Vector3 target, float speed)
    {
        if (isMovingToTarget) yield break;

        isMovingToTarget = true;
        
        while (Vector2.Distance(transform.position, target) > 0.001f)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, target, step);
            yield return new WaitForEndOfFrame();
        }

        isMovingToTarget = false;
    }
    #endregion

    #region Attack
    private IEnumerator MeleeAttack()
	{
        // TODO: Detect if enemy is behind an obstacle and if so, do not damage it

        GameObject armPivot = GetArmPivot();

        yield return new WaitForSeconds(meleeDuration / 2);

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemiesLayer);
        List<Collider2D> results = new List<Collider2D>();
        var arm = armPivot.transform.GetChild(0);
        var col = arm.GetComponent<CircleCollider2D>();
        col.OverlapCollider(filter, results);

        foreach (var enemy in results)
        {
            enemy.GetComponent<Enemy>().Hit(meleeDamage, PlayerColCenter);
        }
	}

    private IEnumerator RangedAttack()
	{
        var col = rangedAtkObject.GetComponent<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemiesLayer);
        List<Collider2D> allEnemiesTouchingAtkCollider = new List<Collider2D>();
        List<Collider2D> newEnemiesTouchingAtkCollider = new List<Collider2D>();
        // Stores for a short time enemies who stopped touching the attack collider to prevent the hit push to cause another hit
        Dictionary<Collider2D, float> enemiesOnDelay = new Dictionary<Collider2D, float>();
        float delay = 0.4f;

        while(isRangedAttacking)
        {
            col.OverlapCollider(filter, newEnemiesTouchingAtkCollider);

            // Update enemies delay dictionary
            foreach (var kvp in new Dictionary<Collider2D, float>(enemiesOnDelay))
            {
                var newValue = kvp.Value - Time.deltaTime;
                if (newValue > 0)
                {
                    enemiesOnDelay[kvp.Key] = newValue;
                }
                else
                {
                    enemiesOnDelay.Remove(kvp.Key);
                }
            }
            
            foreach (var enemy in newEnemiesTouchingAtkCollider)
            {
                if (!allEnemiesTouchingAtkCollider.Contains(enemy) && !enemiesOnDelay.ContainsKey(enemy))
                {
                    enemy.GetComponent<Enemy>().Hit(rangedAtkDamage, rangedAtkObject.position);
                    allEnemiesTouchingAtkCollider.Add(enemy);
                }
            }

            foreach (var enemy in allEnemiesTouchingAtkCollider.ToArray())
            {
                if (!newEnemiesTouchingAtkCollider.Contains(enemy))
                {
                    allEnemiesTouchingAtkCollider.Remove(enemy);
                    enemiesOnDelay.Add(enemy, delay);
                }
            }

            yield return new WaitForEndOfFrame();
        }
	}

    private IEnumerator Ultimate()
	{
        isUltimateAttacking = true;
        ultimateParticles.SetActive(true);

        List<Collider2D> results = new List<Collider2D>();
		ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemiesLayer);

        float timer = ultimateDuration;
        while (timer > 0)
        {
            timer -= ultimateDamageFrequency;
            
            Physics2D.OverlapCircle(PlayerColCenter, ultimateRadius, filter, results);

            foreach (var enemy in results)
            {
                enemy.GetComponent<Enemy>().Hit(ultimateDamage, PlayerColCenter);
            }

            yield return new WaitForSeconds(ultimateDamageFrequency);
        }

        isUltimateAttacking = false;
        ultimateParticles.SetActive(false);
	}

    private GameObject GetArmPivot()
	{
		if (lookingUp) return armPivotUp;

        if (lookingRight) return armPivotRight;

        if (lookingLeft) return armPivotLeft;

        if (lookingDown) return armPivotDown;

        Debug.LogWarning("Could not find correct arm pivot.");
        return armPivotDown;
	}
    #endregion

    #region Health
    public void Hit(int amount, Vector3 pusherPosition=default(Vector3))
    {
        if (isUltimateAttacking) return;
        
        CurrentHealth -= amount;

        if (pusherPosition != Vector3.zero)
        {
            Vector3 dir = (pusherPosition - transform.position).normalized;
            Vector3 target = transform.position - (dir * pushDistance);

            StartCoroutine(MovePlayerTo(target, pushSpeed));
        }

        inHitConfusion = true;
        Invoke("StopConfusion", 0.5f);
        rb.velocity = Vector2.zero;
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
    }

    private void OnDeath()
	{
		SceneManager.LoadScene("Map");
	}
    #endregion

    #region Balance
    public void IncreaseBalance(int amount)
    {
        Balance += amount;
    }

    public void DecreaseBalance(int amount)
    {
        Balance -= amount;
    }
    #endregion

    #region General
    private Vector3 GetMouseDirection()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 mouseDir = (mousePos - (Vector2)transform.position).normalized;
		return (Vector3)mouseDir;
	}
    #endregion
}
