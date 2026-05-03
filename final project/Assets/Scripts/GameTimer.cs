using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public float startTime = 30f;
    private float currentTime;

    public TMP_Text timerText;
    [Header("Sound")]
    public AudioSource tickAudio;
    public float tickStartTime = 10f;

    [Header("Game Over")]
    public GameObject gameOverUI;   // панель с текстом и кнопкой
    public MonoBehaviour playerController; // твой скрипт движения (или весь объект игрока)

    private bool isGameOver;

    void Start()
    {
        currentTime = startTime;

        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            if (currentTime < 0) currentTime = 0;
        }

        UpdateUI();

        if (currentTime <= 0)
        {
            TriggerGameOver();
        }
        
        if (!isGameOver && currentTime <= tickStartTime)
{
    if (tickAudio != null && !tickAudio.isPlaying)
        tickAudio.Play();
}
    }

    void UpdateUI()
    {
        int seconds = Mathf.FloorToInt(currentTime);
        int milliseconds = Mathf.FloorToInt((currentTime - seconds) * 100);
        timerText.text = $"Time: {seconds:00}:{milliseconds:00}";
    }

    public void AddTime(float amount)
    {
        if (isGameOver) return;
        currentTime += amount;
    }

    void TriggerGameOver()
    {
        isGameOver = true;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        // стоп игрока
        if (playerController != null)
            playerController.enabled = false;

        // курсор вернуть
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f; // стоп времени
    }
    public float GetCurrentTime()
{
    return currentTime;
}
}