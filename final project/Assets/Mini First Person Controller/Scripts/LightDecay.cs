using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDecay : MonoBehaviour
{
    public Light torchLight;
    public float decaySpeed = 0.2f;
    public float minRange = 1.5f;
    public float maxRange = 10f;

    void Update()
    {
        if (torchLight.range > minRange)
        {
            torchLight.range -= decaySpeed * Time.deltaTime;
        }
    }

    public void AddLight(float amount)
    {
        torchLight.range += amount;
        torchLight.range = Mathf.Clamp(torchLight.range, minRange, maxRange);
    }
}