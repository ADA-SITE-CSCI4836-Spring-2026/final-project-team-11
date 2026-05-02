using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDecay : MonoBehaviour
{
    public Light torchLight;
    public float decaySpeed = 1f;
    public float minRange = 1.5f;

    void Update()
    {
        if (torchLight.range > minRange)
        {
            torchLight.range -= decaySpeed * Time.deltaTime;
        }
    }
}
