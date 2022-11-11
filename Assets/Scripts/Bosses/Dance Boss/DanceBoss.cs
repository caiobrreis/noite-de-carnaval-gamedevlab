using UnityEngine;

public class DanceBoss : MonoBehaviour
{
    public float keySpeed;
    public int keysAmount;
    public float keysInterval = 0.75f;
    [Range(0f, 1f)] public float doubleKeyProbability;

    public DanceBossManager danceBossManager;

    private float timeUntilNextKey;
    private int keysGenerated = 0;
    [HideInInspector] public int keysEvaluated = 0;

    private bool endedBattle = false;
    public AudioClip audioClip;

	private string currentAnimState;
	private Animator anim;
    const string ANIM_IDLE = "Idle";
    const string ANIM_DANCE = "Dance";

    private void OnEnable() 
    {
        anim = GetComponent<Animator>();
        ChangeAnimationState(ANIM_DANCE);
    }

    private void OnDisable() 
    {
        ChangeAnimationState(ANIM_IDLE);
    }

	void Start()
    {
        timeUntilNextKey = keysInterval;
    }

    void Update()
    {
        if (endedBattle) return;

        if (keysEvaluated == keysAmount)
		{
			Invoke("EndBattle", 2f);
            endedBattle = true;
		}

		if (keysGenerated == keysAmount) return;

		timeUntilNextKey -= Time.deltaTime;

        if (timeUntilNextKey <= 0) {
            timeUntilNextKey = keysInterval;
            keysGenerated++;

            int i = Random.Range(0, danceBossManager.keys.Length);
            SpawnKey(i);
            
            if (Random.Range(0f, 1f) <= doubleKeyProbability) {
                int j = RandomIndexExcludingCurrent(i);
                SpawnKey(j);
            }
        }
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentAnimState == newState) return;

        anim.Play(newState);

        currentAnimState = newState;
    }

	private void EndBattle()
	{
		DestroyActiveKeys();

		danceBossManager.StopBattle();
		this.enabled = false;
	}

	private static void DestroyActiveKeys()
	{
		var activeKeys = FindObjectsOfType<DanceBossKey>();

        for (int i = 0; i < activeKeys.Length; i++)
        {
            Destroy(activeKeys[i].gameObject);
        }
	}

	private void SpawnKey(int index)
	{
		GameObject newKey = Instantiate(danceBossManager.keyPrefab, GameManager.manager.data.keySpawns[index].transform.position, Quaternion.identity, GameManager.manager.data.spawnableKeyParent);
		SetupKey(index, newKey);
	}

	private void SetupKey(int index, GameObject newKey)
	{
		DanceBossKey newKeyScript = newKey.GetComponent<DanceBossKey>();
		newKeyScript.thisKey = danceBossManager.keys[index];
		newKeyScript.speed = keySpeed;
		newKeyScript.danceBoss = this;
	}

	private int RandomIndexExcludingCurrent(int currentIndex)
	{
		return (currentIndex + Random.Range(1, danceBossManager.keys.Length)) % danceBossManager.keys.Length; 
	}
}