using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeUI
{
    [DefaultExecutionOrder(-200)]
    public class UIManager : MonoBehaviour
    {
        private const string WinPanelName = "WinPanel";
        private const string GameOverPanelName = "GameOverPanel";
        private const string InteractionTextName = "InteractionText";

        public GameObject winPanel;
        public GameObject gameOverPanel;

        [SerializeField] private GameObject interactionText;

        private TMP_Text interactionLabel;
        private FirstPersonMovement playerMovement;
        private FirstPersonLook playerLook;
        private bool gameEnded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Time.timeScale = 1f;

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                canvas = CreateCanvas();
            }

            UIManager manager = canvas.GetComponent<UIManager>();
            if (manager == null)
            {
                manager = canvas.gameObject.AddComponent<UIManager>();
            }

            manager.Initialize(canvas);
        }

        public void ShowWin()
        {
            if (gameEnded)
            {
                return;
            }

            gameEnded = true;
            SetInteractionTextVisible(false);

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }

            FreezeGameplay();
        }

        public void ShowGameOver()
        {
            if (gameEnded)
            {
                return;
            }

            gameEnded = true;
            SetInteractionTextVisible(false);

            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            FreezeGameplay();
        }

        public void SetInteractionTextVisible(bool isVisible)
        {
            if (interactionText == null)
            {
                return;
            }

            if (interactionLabel != null)
            {
                interactionLabel.text = "Press E to Escape";
            }

            interactionText.SetActive(isVisible && !gameEnded);
        }

        private void Initialize(Canvas canvas)
        {
            EnsurePanels(canvas.transform);
            CachePlayerControls();
            EnsureGameOverListener();
            EnsureWinTrigger();
        }

        private void EnsurePanels(Transform parent)
        {
            winPanel = winPanel != null ? winPanel : FindChild(parent, WinPanelName);
            gameOverPanel = gameOverPanel != null ? gameOverPanel : FindChild(parent, GameOverPanelName);
            interactionText = interactionText != null ? interactionText : FindChild(parent, InteractionTextName);

            if (winPanel == null)
            {
                winPanel = CreateOverlayPanel(parent, WinPanelName, "YOU ESCAPED");
            }

            if (gameOverPanel == null)
            {
                gameOverPanel = CreateOverlayPanel(parent, GameOverPanelName, "GAME OVER");
            }

            if (interactionText == null)
            {
                interactionText = CreateInteractionText(parent);
            }

            interactionLabel = interactionText.GetComponent<TMP_Text>();
            winPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            interactionText.SetActive(false);
        }

        private void CachePlayerControls()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            playerMovement = player.GetComponent<FirstPersonMovement>();
            playerLook = player.GetComponentInChildren<FirstPersonLook>(true);
        }

        private void FreezeGameplay()
        {
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            if (playerLook != null)
            {
                playerLook.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }

        private void EnsureGameOverListener()
        {
            GameTimer timer = Object.FindObjectOfType<GameTimer>();
            if (timer == null)
            {
                return;
            }

            GameOverListener listener = timer.GetComponent<GameOverListener>();
            if (listener == null)
            {
                listener = timer.gameObject.AddComponent<GameOverListener>();
            }

            listener.SetUIManager(this);
        }

        private void EnsureWinTrigger()
        {
            if (Object.FindObjectOfType<WinTrigger>() != null)
            {
                return;
            }

            Vector3 triggerPosition = new Vector3(8.8f, 1.5f, -6.2f);
            Light orangeRoomLight = GameObject.Find("Spot Light")?.GetComponent<Light>();
            if (orangeRoomLight != null)
            {
                triggerPosition = orangeRoomLight.transform.position;
                triggerPosition.y = 1.5f;
            }

            GameObject triggerObject = new GameObject("OrangeRoomEscapeTrigger");
            triggerObject.transform.position = triggerPosition;

            BoxCollider boxCollider = triggerObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(9f, 3f, 9f);

            WinTrigger winTrigger = triggerObject.AddComponent<WinTrigger>();
            winTrigger.SetUIManager(this);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateOverlayPanel(Transform parent, string panelName, string title)
        {
            GameObject panel = new GameObject(panelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image background = panel.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.8f);

            CreatePanelLabel(panel.transform, title);
            CreateRestartButton(panel.transform);
            return panel;
        }

        private static void CreatePanelLabel(Transform parent, string labelText)
        {
            GameObject label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            label.transform.SetParent(parent, false);

            RectTransform rectTransform = label.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, 80f);
            rectTransform.sizeDelta = new Vector2(700f, 120f);

            TMP_Text text = label.GetComponent<TextMeshProUGUI>();
            text.text = labelText;
            text.fontSize = 54f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateRestartButton(Transform parent)
        {
            GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RestartButton));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, -40f);
            rectTransform.sizeDelta = new Vector2(220f, 64f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            colors.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 0.95f);
            colors.pressedColor = new Color(0.35f, 0.35f, 0.35f, 0.95f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.5f);
            button.colors = colors;

            RestartButton restartButton = buttonObject.GetComponent<RestartButton>();
            button.onClick.AddListener(restartButton.RestartScene);

            GameObject label = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            label.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TMP_Text labelText = label.GetComponent<TextMeshProUGUI>();
            labelText.text = "Restart";
            labelText.fontSize = 28f;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Center;
        }

        private static GameObject CreateInteractionText(Transform parent)
        {
            GameObject interaction = new GameObject(InteractionTextName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            interaction.transform.SetParent(parent, false);

            RectTransform rectTransform = interaction.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, 70f);
            rectTransform.sizeDelta = new Vector2(500f, 60f);

            TMP_Text text = interaction.GetComponent<TextMeshProUGUI>();
            text.text = "Press E to Escape";
            text.fontSize = 32f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            return interaction;
        }

        private static GameObject FindChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            return child != null ? child.gameObject : null;
        }
    }
}
