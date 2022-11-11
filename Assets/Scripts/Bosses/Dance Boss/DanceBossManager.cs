using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DanceBossManager : MonoBehaviour
{
    public AudioSource audioSource;
    public DialogueNoInteraction dialogueNoInteraction;
    public DialogueScriptableObject[] dialogues;
    public GameObject[] bosses = new GameObject[3];
    public KeyCode[] keys = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    public GameObject keyPrefab;
	[SerializeField] private float startBattleDelay = 3f;

    private GameObject player;
    private Player playerScript;

    private int currentBossIndex = 0;
    private int currentDialogueIndex = 0;

    private enum PlayerState
    {
        InDialogue,
        Moving,
        InBattle,
    }

    private PlayerState currentPlayerState;

	void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<Player>();
    }

    void Start()
    {
        dialogueNoInteraction.StartDialogue(dialogues[currentDialogueIndex++]);
        currentPlayerState = PlayerState.InDialogue;
    }

    void Update()
    {
        playerScript.CanBeControlled = false;

        if (currentPlayerState == PlayerState.InBattle) return;

        if (currentPlayerState == PlayerState.InDialogue)
        {
            if (dialogueNoInteraction.isOver)
            {
				if (currentBossIndex > bosses.Length - 1)
				{
                    GameManager.manager.SaveDataAndLoadScene("Map");
					return;
				}
                else
                {
                    currentPlayerState = PlayerState.Moving;
                }
            }
        }

        if (currentPlayerState == PlayerState.Moving)
        {
            if (MoveToBoss(currentBossIndex))
            {
                currentPlayerState = PlayerState.InBattle;
                StartCoroutine(StartBattle());
            }
        }
    }

	private bool MoveToBoss(int bossIndex)
	{
		Vector2 target = new Vector2(bosses[bossIndex].transform.position.x, player.transform.position.y);
		return playerScript.MoveToWithReturn(target);
	}

	private IEnumerator StartBattle()
	{
		GameManager.manager.data.danceBossScreen.SetActive(true);
        var danceBossScript = bosses[currentBossIndex].GetComponent<DanceBoss>();
        audioSource.PlayOneShot(danceBossScript.audioClip);

        yield return new WaitForSeconds(startBattleDelay);

        danceBossScript.enabled = true;
        playerScript.StartDancing();
	}

    public void StopBattle()
	{
		GameManager.manager.data.danceBossScreen.SetActive(false);
        playerScript.StopDancing();
        audioSource.Stop();
        currentBossIndex++;

        dialogueNoInteraction.StartDialogue(dialogues[currentDialogueIndex++]);
        currentPlayerState = PlayerState.InDialogue;
	}
}
