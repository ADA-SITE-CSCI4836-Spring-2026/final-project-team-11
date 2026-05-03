using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeUI
{
    public class RestartButton : MonoBehaviour
    {
        public void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
