using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/DialogueScriptableObject")]
public class DialogueScriptableObject : ScriptableObject
{
    public string content;
    public string speakerName;
    public Sprite icon;
    public DialogueScriptableObject nextDialogue;
}
