using UnityEngine;

public class GoToDanceBossScene : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            GameManager.manager.SaveDataAndLoadScene("DanceBoss");
        }
    }
}
