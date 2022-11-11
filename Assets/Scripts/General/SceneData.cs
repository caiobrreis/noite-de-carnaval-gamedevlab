using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SceneData : MonoBehaviour
{
    [Header("Health")]
    public Transform healthContainer;
    public GameObject heartPrefab;

    [Space(5)]

    [Header("Balance")]
    public Transform coinContainer;

    [Space(5)]

    [Header("Dialogue")]
    public GameObject dialogueScreen;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI dialogueSpeaker;
    public Image dialogueIcon;

    [Space(5)]

    [Header("Dance Boss")]
    public GameObject danceBossScreen;
    public Transform spawnableKeyParent;
    public RectTransform keyHitBarRect;
    public Transform[] keySpawns = new Transform[4];
}
