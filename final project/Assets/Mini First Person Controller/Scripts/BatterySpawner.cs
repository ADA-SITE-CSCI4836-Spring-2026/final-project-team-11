using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartBatterySpawner : MonoBehaviour
{
    public GameObject batteryPrefab;
    public int count = 30;

    public float spawnHeight = 10f;

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-50f, 50f),
                spawnHeight,
                Random.Range(-50f, 50f)
            );

            RaycastHit hit;

            if (Physics.Raycast(randomPos, Vector3.down, out hit, 20f))
            {
                if (hit.collider.CompareTag("Floor"))
                {
                    Instantiate(
                        batteryPrefab,
                        hit.point + Vector3.up * 0.5f,
                        Quaternion.identity
                    );
                    return;
                }
            }
        }
    }
}