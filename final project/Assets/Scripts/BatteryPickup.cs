using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    [SerializeField] private float rangeRestore = 4f;
    [SerializeField] private float rotationSpeed = 90f;

    private bool collected;

    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
        {
            return;
        }

        var lightDecay = other.GetComponentInParent<LightDecay>();
        if (lightDecay == null || lightDecay.torchLight == null)
        {
            return;
        }

        lightDecay.torchLight.range += rangeRestore;

        GameTimer gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer != null)
        {
            gameTimer.AddTime(5f);
        }

        collected = true;
        Destroy(gameObject);
    }
}
