using UnityEngine;

namespace EscapeUI
{
    public class GameOverListener : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;

        private GameTimer gameTimer;
        private bool hasTriggered;

        private void Awake()
        {
            gameTimer = GetComponent<GameTimer>();
            if (uiManager == null)
            {
                uiManager = Object.FindObjectOfType<UIManager>(true);
            }
        }

        private void Update()
        {
            if (hasTriggered || gameTimer == null)
            {
                return;
            }

            if (gameTimer.GetCurrentTime() <= 0f)
            {
                TriggerGameOver();
            }
        }

        public void SetUIManager(UIManager manager)
        {
            uiManager = manager;
        }

        public void TriggerGameOver()
        {
            if (hasTriggered)
            {
                return;
            }

            hasTriggered = true;

            if (uiManager == null)
            {
                uiManager = Object.FindObjectOfType<UIManager>(true);
            }

            if (uiManager != null)
            {
                uiManager.ShowGameOver();
            }
        }
    }
}
