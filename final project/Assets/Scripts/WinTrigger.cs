using UnityEngine;

namespace EscapeUI
{
    [RequireComponent(typeof(BoxCollider))]
    public class WinTrigger : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;

        private bool playerInside;

        private void Awake()
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;

            if (uiManager == null)
            {
                uiManager = Object.FindObjectOfType<UIManager>(true);
            }
        }

        private void Update()
        {
            if (!playerInside || uiManager == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                playerInside = false;
                uiManager.SetInteractionTextVisible(false);
                uiManager.ShowWin();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other))
            {
                return;
            }

            playerInside = true;
            uiManager?.SetInteractionTextVisible(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other))
            {
                return;
            }

            playerInside = false;
            uiManager?.SetInteractionTextVisible(false);
        }

        public void SetUIManager(UIManager manager)
        {
            uiManager = manager;
        }

        private static bool IsPlayer(Collider other)
        {
            Transform root = other.transform.root;
            return (root != null && root.CompareTag("Player")) || other.CompareTag("Player");
        }
    }
}
