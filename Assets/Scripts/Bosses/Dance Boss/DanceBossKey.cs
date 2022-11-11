using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DanceBossKey : MonoBehaviour
{
    private bool scored = false;

    [HideInInspector] public KeyCode thisKey;
    [HideInInspector] public float speed;
    [HideInInspector] public DanceBoss danceBoss;

    private RectTransform thisRect;
    private TextMeshProUGUI keyText;
    private Player player;

    private enum State
    {
        BeforeHitBar,
        OverHitBar,
        AfterHitBar
    }

    private State currentState = State.BeforeHitBar;

    private void Awake() 
    {
        player = FindObjectOfType<Player>();
    }

    void Start() 
    {
        Destroy(gameObject, 5f);
        
        thisRect = GetComponent<RectTransform>();
        keyText = GetComponent<TextMeshProUGUI>();

        keyText.text = thisKey.ToString();
    }

    void Update()
    {
        MoveKeyDownwards(speed * Time.deltaTime);

        if (scored) return;
        if (currentState == State.AfterHitBar) return;

        if (currentState == State.BeforeHitBar)
        {
            if (GameManager.manager.data.keyHitBarRect.Overlaps(thisRect))
            {
                currentState = State.OverHitBar;
            }
        }

        if (currentState == State.OverHitBar)
        {
            if (GameManager.manager.data.keyHitBarRect.Overlaps(thisRect))
            {
                if ((Input.GetKey(thisKey) && Input.GetMouseButton(0)))
                {
                    scored = true;
                    keyText.color = Color.green;
                    danceBoss.keysEvaluated++;
                }
            }
            else
            {
                currentState = State.AfterHitBar;
                keyText.color = Color.red;
                danceBoss.keysEvaluated++;
                player.Hit(1);
            }
        }
    }

	private void MoveKeyDownwards(float amount)
	{
		transform.position = new Vector2(transform.position.x, transform.position.y - amount);
	}
}
