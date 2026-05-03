using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    private UIManager uiManager;
    private bool playerInside = false;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            uiManager?.ShowWin();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            uiManager?.SetInteractionTextVisible(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            uiManager?.SetInteractionTextVisible(false);
        }
    }
}
