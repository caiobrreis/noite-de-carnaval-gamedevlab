using UnityEngine;

public class DialogueInteraction : MonoBehaviour
{
    public DialogueScriptableObject firstDialogue;
    private DialogueScriptableObject currDialogue;
    private bool playerInRange = false;

    public GameObject keyPrefab;
    private GameObject keyCreated;
    public float keyYOffset;

    private Player playerScript;

    private void Update() 
    {
        if (playerInRange) {
            if (Input.GetKeyDown(KeyCode.E)) {
                if (currDialogue == null) {
                    GameManager.manager.data.dialogueScreen.SetActive(false);
                    playerScript.CanBeControlled = true;
                    currDialogue = firstDialogue;
                } else {
                    GameManager.manager.data.dialogueScreen.SetActive(true);
                    GameManager.manager.data.dialogueText.text = currDialogue.content;
                    playerScript.CanBeControlled = false;
                    currDialogue = currDialogue.nextDialogue;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player")) {
            keyCreated = Instantiate(keyPrefab, transform.position + new Vector3(0f, keyYOffset, 0f), Quaternion.identity, transform);
            currDialogue = firstDialogue;
            playerInRange = true;
            playerScript = other.GetComponent<Player>();
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.CompareTag("Player")) {
            Destroy(keyCreated);
            playerInRange = false;
        }
    }
}
