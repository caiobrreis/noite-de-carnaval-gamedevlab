using UnityEngine;

public class DialogueNoInteraction : MonoBehaviour
{
    [HideInInspector] public bool isOver = false;
    
    private DialogueScriptableObject firstDialogue;
    private DialogueScriptableObject currDialogue;

    private Player playerScript;

    void Start()
	{
		playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
	}

	void Update()
	{
        if (isOver) return;
        
		if (Input.GetKeyDown(KeyCode.E))
		{
			if (currDialogue == null)
			{
				StopDialogue();
			} 
			else
			{
				NextDialogue();
			}
		}
	}

	private void NextDialogue()
	{
		ChangeDialogueContent(currDialogue);
		currDialogue = currDialogue.nextDialogue;
	}

	private void ChangeDialogueContent(DialogueScriptableObject dialogue)
	{
		GameManager.manager.data.dialogueText.text = dialogue.content;
		GameManager.manager.data.dialogueSpeaker.text = dialogue.speakerName;
		GameManager.manager.data.dialogueIcon.sprite = dialogue.icon;
	}

	public void StartDialogue(DialogueScriptableObject dialogue)
	{
		firstDialogue = dialogue;

		GameManager.manager.data.dialogueScreen.SetActive(true);
		ChangeDialogueContent(firstDialogue);

		currDialogue = firstDialogue.nextDialogue;
		playerScript.CanBeControlled = false;
        isOver = false;
	}

	private void StopDialogue()
	{
		GameManager.manager.data.dialogueScreen.SetActive(false);
		playerScript.CanBeControlled = true;
		isOver = true;
	}
}
