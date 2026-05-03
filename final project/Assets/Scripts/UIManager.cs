using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject gameOverPanel;
    public GameObject interactionText;

    public void ShowWin()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (interactionText != null) interactionText.SetActive(false);

        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (interactionText != null) interactionText.SetActive(false);

        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void SetInteractionTextVisible(bool visible)
    {
        if (interactionText != null)
            interactionText.SetActive(visible);
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
