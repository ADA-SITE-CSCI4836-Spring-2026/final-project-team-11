using UnityEngine;

namespace MiniFirstPersonController
{
    public class BatteryPickup : MonoBehaviour
    {
        public float lightAmount = 2f;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                LightDecay ld = FindObjectOfType<LightDecay>();

                if (ld != null)
                {
                    ld.AddLight(lightAmount);
                }

                Destroy(gameObject);
            }
        }
    }
}
