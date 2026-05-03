using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffect : MonoBehaviour
{
    public Image redOverlay;
    public GameTimer timer;

    public float maxAlpha = 0.6f;
    public float dangerTime = 10f; // когда начинается краснение

    void Update()
    {
        if (timer == null || redOverlay == null) return;

        float t = timerTimeNormalized();
        float alpha = Mathf.Lerp(0f, maxAlpha, t);

        Color c = redOverlay.color;
        c.a = alpha;
        redOverlay.color = c;
    }

    float timerTimeNormalized()
    {
        // чем меньше времени — тем больше красный
        float ratio = Mathf.Clamp01(1f - (timerCurrent() / timerStart()));
        return ratio;
    }

    float timerCurrent()
    {
        return (float)typeof(GameTimer)
            .GetField("currentTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(timer);
    }

    float timerStart()
    {
        return timer.startTime;
    }
}
