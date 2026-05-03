using UnityEngine;
using UnityEngine.UI;

public class ScreenEffect : MonoBehaviour
{
    public Image redOverlay;
    public GameTimer timer;

    public float maxAlpha = 0.5f;
    public float dangerTime = 7f; // 🔥 только последние 5 секунд

    void Update()
    {
        if (timer == null || redOverlay == null) return;

        float currentTime = timer.GetCurrentTime();

        float t = 0f;

        if (currentTime <= dangerTime)
        {
            t = 1f - (currentTime / dangerTime);

            // плавное нарастание (не резко)
            t = Mathf.Pow(t, 2f);
        }

        float alpha = Mathf.Lerp(0f, maxAlpha, t);

        Color c = redOverlay.color;
        c.a = alpha;
        redOverlay.color = c;
    }

    public void DisableEffect()
    {
        if (redOverlay != null)
        {
            Color c = redOverlay.color;
            c.a = 0f;
            redOverlay.color = c;
        }

        enabled = false;
    }
}